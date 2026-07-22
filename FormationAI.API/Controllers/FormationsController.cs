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
public class FormationsController : ControllerBase
{
    private readonly AppDbContext _db;
    public FormationsController(AppDbContext db) => _db = db;

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Liste publique des formations publiées
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<FormationDto>>> GetAll()
    {
        var formations = await _db.Formations
            .Include(f => f.Formateur)
            .Include(f => f.Categorie)
            .Where(f => f.Publiee)
            .Select(f => new FormationDto(f.Id, f.Titre, f.Description,
                f.CategorieId, f.Categorie != null ? f.Categorie.Nom : null,
                f.DureeHeures, f.Prix, f.Publiee, f.FormateurId,
                f.Formateur!.Prenom + " " + f.Formateur.Nom))
            .ToListAsync();
        return Ok(formations);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<FormationDto>> GetById(int id)
    {
        var f = await _db.Formations
            .Include(x => x.Formateur)
            .Include(x => x.Categorie)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (f is null) return NotFound();
        return Ok(new FormationDto(f.Id, f.Titre, f.Description,
            f.CategorieId, f.Categorie?.Nom,
            f.DureeHeures, f.Prix, f.Publiee, f.FormateurId,
            f.Formateur!.Prenom + " " + f.Formateur.Nom));
    }

    [HttpPost]
    [Authorize(Roles = "Formateur,Admin")]
    public async Task<ActionResult<FormationDto>> Create(FormationCreateDto dto)
    {
        var formation = new Formation
        {
            Titre = dto.Titre,
            Description = dto.Description,
            CategorieId = dto.CategorieId,
            DureeHeures = dto.DureeHeures,
            Prix = dto.Prix,
            FormateurId = UserId
        };
        _db.Formations.Add(formation);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = formation.Id },
            new FormationDto(formation.Id, formation.Titre, formation.Description,
                formation.CategorieId, null, formation.DureeHeures, formation.Prix,
                formation.Publiee, formation.FormateurId, ""));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Formateur,Admin")]
    public async Task<IActionResult> Update(int id, FormationUpdateDto dto)
    {
        var formation = await _db.Formations.FindAsync(id);
        if (formation is null) return NotFound();
        if (formation.FormateurId != UserId && !User.IsInRole("Admin"))
            return Forbid();

        formation.Titre = dto.Titre;
        formation.Description = dto.Description;
        formation.CategorieId = dto.CategorieId;
        formation.DureeHeures = dto.DureeHeures;
        formation.Prix = dto.Prix;
        formation.Publiee = dto.Publiee;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Formateur,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var formation = await _db.Formations.FindAsync(id);
        if (formation is null) return NotFound();
        if (formation.FormateurId != UserId && !User.IsInRole("Admin"))
            return Forbid();

        _db.Formations.Remove(formation);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
