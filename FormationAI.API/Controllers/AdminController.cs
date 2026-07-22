using FormationAI.API.Data;
using FormationAI.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FormationAI.API.Controllers;

// Interface d'administration : gestion des utilisateurs et des formateurs.
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;
    public AdminController(AppDbContext db) => _db = db;

    [HttpGet("utilisateurs")]
    public async Task<ActionResult<IEnumerable<object>>> Utilisateurs([FromQuery] UserRole? role)
    {
        var query = _db.Users.AsQueryable();
        if (role is not null) query = query.Where(u => u.Role == role);

        var data = await query
            .Select(u => new { u.Id, u.Prenom, u.Nom, u.Email, Role = u.Role.ToString(), u.DateCreation })
            .ToListAsync();
        return Ok(data);
    }

    // Liste dédiée des formateurs avec le nombre de formations animées
    [HttpGet("formateurs")]
    public async Task<ActionResult<IEnumerable<object>>> Formateurs()
    {
        var data = await _db.Users
            .Where(u => u.Role == UserRole.Formateur)
            .Select(u => new
            {
                u.Id,
                Nom = u.Prenom + " " + u.Nom,
                u.Email,
                NbFormations = u.FormationsAnimees.Count
            })
            .ToListAsync();
        return Ok(data);
    }

    // Changer le rôle d'un utilisateur (ex : promouvoir un apprenant en formateur)
    [HttpPatch("utilisateurs/{id:int}/role")]
    public async Task<IActionResult> ChangerRole(int id, [FromQuery] UserRole role)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();
        user.Role = role;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // Tableau de bord : quelques statistiques
    [HttpGet("statistiques")]
    public async Task<ActionResult<object>> Statistiques() => Ok(new
    {
        Utilisateurs = await _db.Users.CountAsync(),
        Formateurs = await _db.Users.CountAsync(u => u.Role == UserRole.Formateur),
        Apprenants = await _db.Users.CountAsync(u => u.Role == UserRole.Apprenant),
        Formations = await _db.Formations.CountAsync(),
        Categories = await _db.Categories.CountAsync(),
        Inscriptions = await _db.Inscriptions.CountAsync(),
        Certificats = await _db.Certificats.CountAsync(),
        VideosIA = await _db.AvatarVideos.CountAsync()
    });
}
