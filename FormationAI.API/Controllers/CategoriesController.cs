using FormationAI.API.Data;
using FormationAI.API.DTOs;
using FormationAI.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FormationAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;
    public CategoriesController(AppDbContext db) => _db = db;

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<CategorieDto>>> GetAll() =>
        Ok(await _db.Categories
            .Select(c => new CategorieDto(c.Id, c.Nom, c.Description))
            .ToListAsync());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategorieDto>> Create(CategorieCreateDto dto)
    {
        var c = new Categorie { Nom = dto.Nom, Description = dto.Description };
        _db.Categories.Add(c);
        await _db.SaveChangesAsync();
        return Ok(new CategorieDto(c.Id, c.Nom, c.Description));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await _db.Categories.FindAsync(id);
        if (c is null) return NotFound();
        _db.Categories.Remove(c);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
