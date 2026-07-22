using System.Security.Claims;
using FormationAI.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FormationAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CertificatsController : ControllerBase
{
    private readonly AppDbContext _db;
    public CertificatsController(AppDbContext db) => _db = db;

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("mes-certificats")]
    public async Task<ActionResult<IEnumerable<object>>> MesCertificats()
    {
        var data = await _db.Certificats
            .Where(c => c.ApprenantId == UserId)
            .Include(c => c.Formation)
            .Select(c => new
            {
                c.Id,
                c.Reference,
                Formation = c.Formation!.Titre,
                c.Note,
                c.DateEmission
            })
            .ToListAsync();
        return Ok(data);
    }

    // Vérification publique d'un certificat par sa référence
    [HttpGet("verifier/{reference}")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> Verifier(string reference)
    {
        var c = await _db.Certificats
            .Include(x => x.Formation)
            .Include(x => x.Apprenant)
            .FirstOrDefaultAsync(x => x.Reference == reference);
        if (c is null) return NotFound(new { valide = false });

        return Ok(new
        {
            valide = true,
            c.Reference,
            Apprenant = c.Apprenant!.Prenom + " " + c.Apprenant.Nom,
            Formation = c.Formation!.Titre,
            c.Note,
            c.DateEmission
        });
    }
}
