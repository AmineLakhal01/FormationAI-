using FormationAI.API.Data;
using FormationAI.API.DTOs;
using FormationAI.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FormationAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionsController : ControllerBase
{
    private readonly AppDbContext _db;
    public SessionsController(AppDbContext db) => _db = db;

    [HttpGet("formation/{formationId:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<SessionFormation>>> GetByFormation(int formationId)
    {
        var sessions = await _db.Sessions
            .Where(s => s.FormationId == formationId)
            .OrderBy(s => s.DateDebut)
            .ToListAsync();
        return Ok(sessions);
    }

    [HttpPost]
    [Authorize(Roles = "Formateur,Admin")]
    public async Task<ActionResult<SessionFormation>> Create(SessionCreateDto dto)
    {
        if (!await _db.Formations.AnyAsync(f => f.Id == dto.FormationId))
            return BadRequest(new { message = "Formation introuvable." });

        var session = new SessionFormation
        {
            Titre = dto.Titre,
            DateDebut = dto.DateDebut,
            DateFin = dto.DateFin,
            PlacesMax = dto.PlacesMax,
            Lieu = dto.Lieu,
            FormationId = dto.FormationId
        };
        _db.Sessions.Add(session);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetByFormation), new { formationId = dto.FormationId }, session);
    }

    [HttpPatch("{id:int}/statut")]
    [Authorize(Roles = "Formateur,Admin")]
    public async Task<IActionResult> ChangerStatut(int id, [FromQuery] StatutSession statut)
    {
        var session = await _db.Sessions.FindAsync(id);
        if (session is null) return NotFound();
        session.Statut = statut;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
