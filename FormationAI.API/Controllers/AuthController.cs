using FormationAI.API.Data;
using FormationAI.API.DTOs;
using FormationAI.API.Models;
using FormationAI.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FormationAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokens;

    public AuthController(AppDbContext db, ITokenService tokens)
    {
        _db = db;
        _tokens = tokens;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            return Conflict(new { message = "Un compte existe déjà avec cet email." });

        var user = new User
        {
            Nom = dto.Nom,
            Prenom = dto.Prenom,
            Email = dto.Email,
            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(dto.MotDePasse),
            Role = dto.Role
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var (token, exp) = _tokens.GenererToken(user);
        return Ok(new AuthResponseDto(token, exp, user.Id, user.Email, user.Role.ToString()));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.MotDePasse, user.MotDePasseHash))
            return Unauthorized(new { message = "Email ou mot de passe incorrect." });

        var (token, exp) = _tokens.GenererToken(user);
        return Ok(new AuthResponseDto(token, exp, user.Id, user.Email, user.Role.ToString()));
    }
}
