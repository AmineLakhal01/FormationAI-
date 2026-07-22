using System.Security.Claims;
using FormationAI.API.Data;
using FormationAI.API.DTOs;
using FormationAI.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FormationAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InscriptionsController : ControllerBase
{
    private readonly AppDbContext _db;
    public InscriptionsController(AppDbContext db) => _db = db;

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Un apprenant s'inscrit à une session
    [HttpPost]
    public async Task<ActionResult<Inscription>> Sinscrire(InscriptionCreateDto dto)
    {
        var session = await _db.Sessions
            .Include(s => s.Inscriptions)
            .FirstOrDefaultAsync(s => s.Id == dto.SessionFormationId);
        if (session is null) return NotFound(new { message = "Session introuvable." });

        if (session.Inscriptions.Count >= session.PlacesMax)
            return BadRequest(new { message = "Session complète." });

        if (await _db.Inscriptions.AnyAsync(i =>
                i.ApprenantId == UserId && i.SessionFormationId == dto.SessionFormationId))
            return Conflict(new { message = "Vous êtes déjà inscrit à cette session." });

        var inscription = new Inscription
        {
            ApprenantId = UserId,
            SessionFormationId = dto.SessionFormationId,
            Statut = StatutInscription.Confirmee,
            Progression = new Progression { Pourcentage = 0 }
        };
        _db.Inscriptions.Add(inscription);
        await _db.SaveChangesAsync();
        return Ok(inscription);
    }

    // Mes inscriptions
    [HttpGet("mes-inscriptions")]
    public async Task<ActionResult<IEnumerable<object>>> MesInscriptions()
    {
        var data = await _db.Inscriptions
            .Where(i => i.ApprenantId == UserId)
            .Include(i => i.SessionFormation).ThenInclude(s => s!.Formation)
            .Include(i => i.Progression)
            .Select(i => new
            {
                i.Id,
                i.Statut,
                Session = i.SessionFormation!.Titre,
                Formation = i.SessionFormation.Formation!.Titre,
                Progression = i.Progression!.Pourcentage
            })
            .ToListAsync();
        return Ok(data);
    }

    // Mettre à jour sa progression
    [HttpPatch("{id:int}/progression")]
    public async Task<IActionResult> MajProgression(int id, ProgressionUpdateDto dto)
    {
        var inscription = await _db.Inscriptions
            .Include(i => i.Progression)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (inscription is null) return NotFound();
        if (inscription.ApprenantId != UserId) return Forbid();

        inscription.Progression ??= new Progression { InscriptionId = id };
        inscription.Progression.Pourcentage = Math.Clamp(dto.Pourcentage, 0, 100);
        inscription.Progression.Termine = dto.Pourcentage >= 100;
        inscription.Progression.DerniereMiseAJour = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
