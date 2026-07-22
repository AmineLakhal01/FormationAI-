using FormationAI.API.Data;
using FormationAI.API.DTOs;
using FormationAI.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FormationAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ModulesController : ControllerBase
{
    private readonly AppDbContext _db;
    public ModulesController(AppDbContext db) => _db = db;

    // Arborescence complète d'une formation : modules + contenus ordonnés
    [HttpGet("formation/{formationId:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> GetByFormation(int formationId)
    {
        var modules = await _db.Modules
            .Where(m => m.FormationId == formationId)
            .OrderBy(m => m.Ordre)
            .Select(m => new
            {
                m.Id,
                m.Titre,
                m.Description,
                m.Ordre,
                Contenus = m.Contenus.OrderBy(c => c.Ordre).Select(c => new
                {
                    c.Id,
                    c.Titre,
                    Modalite = c.Modalite.ToString(),
                    c.Ordre,
                    c.Corps,
                    c.AvatarVideoId,
                    c.QuizId
                })
            })
            .ToListAsync();
        return Ok(modules);
    }

    [HttpPost]
    [Authorize(Roles = "Formateur,Admin")]
    public async Task<ActionResult<Module>> CreateModule(ModuleCreateDto dto)
    {
        if (!await _db.Formations.AnyAsync(f => f.Id == dto.FormationId))
            return BadRequest(new { message = "Formation introuvable." });

        var module = new Module
        {
            Titre = dto.Titre,
            Description = dto.Description,
            Ordre = dto.Ordre,
            FormationId = dto.FormationId
        };
        _db.Modules.Add(module);
        await _db.SaveChangesAsync();
        return Ok(new { module.Id, module.Titre });
    }

    [HttpPost("contenus")]
    [Authorize(Roles = "Formateur,Admin")]
    public async Task<ActionResult<Contenu>> CreateContenu(ContenuCreateDto dto)
    {
        if (!await _db.Modules.AnyAsync(m => m.Id == dto.ModuleId))
            return BadRequest(new { message = "Module introuvable." });

        var contenu = new Contenu
        {
            Titre = dto.Titre,
            Modalite = (Modalite)dto.Modalite,
            Ordre = dto.Ordre,
            Corps = dto.Corps,
            AvatarVideoId = dto.AvatarVideoId,
            QuizId = dto.QuizId,
            ModuleId = dto.ModuleId
        };
        _db.Contenus.Add(contenu);
        await _db.SaveChangesAsync();
        return Ok(new { contenu.Id, contenu.Titre, Modalite = contenu.Modalite.ToString() });
    }
}
