using System.ComponentModel.DataAnnotations;
using FormationAI.API.Models;

namespace FormationAI.API.DTOs;

public record RegisterDto(
    [Required] string Nom,
    [Required] string Prenom,
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string MotDePasse,
    UserRole Role = UserRole.Apprenant);

public record LoginDto(
    [Required, EmailAddress] string Email,
    [Required] string MotDePasse);

public record AuthResponseDto(
    string Token,
    DateTime Expiration,
    int UserId,
    string Email,
    string Role);
