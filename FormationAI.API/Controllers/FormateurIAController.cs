using FormationAI.API.Data;
using FormationAI.API.DTOs;
using FormationAI.API.Models;
using FormationAI.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FormationAI.API.Controllers;

// Contrôleur du "Formateur IA" :
//  - HeyGen : génération asynchrone de vidéos avatar à partir d'un script
//  - anam.ai : création d'un session token pour un avatar conversationnel temps réel
[ApiController]
[Route("api/formateur-ia")]
[Authorize]
public class FormateurIAController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IHeyGenService _heyGen;
    private readonly IAnamService _anam;

    public FormateurIAController(AppDbContext db, IHeyGenService heyGen, IAnamService anam)
    {
        _db = db;
        _heyGen = heyGen;
        _anam = anam;
    }

    // 1) Demander la génération d'une vidéo de formateur IA (HeyGen)
    [HttpPost("videos/generer")]
    [Authorize(Roles = "Formateur,Admin")]
    public async Task<ActionResult<VideoStatutDto>> GenererVideo(GenererVideoDto dto)
    {
        if (!await _db.Formations.AnyAsync(f => f.Id == dto.FormationId))
            return BadRequest(new { message = "Formation introuvable." });

        var result = await _heyGen.GenererVideoAsync(dto.Script, dto.AvatarId, dto.VoiceId);

        var video = new AvatarVideo
        {
            Titre = dto.Titre,
            ScriptTexte = dto.Script,
            HeyGenVideoId = result.VideoId,
            Statut = StatutVideo.EnCours,
            FormationId = dto.FormationId
        };
        _db.AvatarVideos.Add(video);
        await _db.SaveChangesAsync();

        return Ok(new VideoStatutDto(video.Id, video.Titre, video.Statut.ToString(),
            video.VideoUrl, video.HeyGenVideoId));
    }

    // 2) Rafraîchir le statut auprès de HeyGen et le persister
    [HttpGet("videos/{id:int}/statut")]
    public async Task<ActionResult<VideoStatutDto>> ObtenirStatutVideo(int id)
    {
        var video = await _db.AvatarVideos.FindAsync(id);
        if (video is null) return NotFound();

        if (video.Statut == StatutVideo.EnCours && video.HeyGenVideoId is not null)
        {
            var statut = await _heyGen.ObtenirStatutAsync(video.HeyGenVideoId);
            video.Statut = statut.Statut switch
            {
                "completed" => StatutVideo.Terminee,
                "failed" => StatutVideo.Echec,
                "processing" or "pending" or "waiting" => StatutVideo.EnCours,
                _ => video.Statut
            };
            if (statut.VideoUrl is not null) video.VideoUrl = statut.VideoUrl;
            await _db.SaveChangesAsync();
        }

        return Ok(new VideoStatutDto(video.Id, video.Titre, video.Statut.ToString(),
            video.VideoUrl, video.HeyGenVideoId));
    }

    // 3) Lister les vidéos d'une formation
    [HttpGet("videos/formation/{formationId:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<VideoStatutDto>>> VideosFormation(int formationId)
    {
        var videos = await _db.AvatarVideos
            .Where(v => v.FormationId == formationId)
            .Select(v => new VideoStatutDto(v.Id, v.Titre, v.Statut.ToString(), v.VideoUrl, v.HeyGenVideoId))
            .ToListAsync();
        return Ok(videos);
    }

    // 4) Démarrer une session conversationnelle temps réel (anam.ai)
    // Renvoie un session token éphémère que le front utilise avec le SDK anam.
    [HttpPost("session-temps-reel")]
    public async Task<ActionResult<SessionIaTokenDto>> DemarrerSessionTempsReel(DemarrerSessionIADto dto)
    {
        var formation = await _db.Formations.FindAsync(dto.FormationId);
        if (formation is null) return BadRequest(new { message = "Formation introuvable." });

        var prompt = dto.SystemPrompt ??
            $"Tu es le formateur IA de la formation « {formation.Titre} ». " +
            $"Sujet : {formation.Description}. Réponds de façon claire et pédagogique.";

        var result = await _anam.CreerSessionTokenAsync(dto.Persona, prompt);
        return Ok(new SessionIaTokenDto(result.SessionToken, result.PersonaId,
            DateTime.UtcNow.AddMinutes(5)));
    }
}
