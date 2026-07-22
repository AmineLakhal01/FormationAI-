using System.ComponentModel.DataAnnotations;

namespace FormationAI.API.DTOs;

// --- HeyGen : génération de vidéo avatar ---
public record GenererVideoDto(
    [Required] int FormationId,
    [Required] string Titre,
    [Required] string Script,
    string? AvatarId,
    string? VoiceId);

public record VideoStatutDto(
    int Id,
    string Titre,
    string Statut,
    string? VideoUrl,
    string? HeyGenVideoId);

// --- anam.ai : session temps réel ---
public record DemarrerSessionIADto(
    [Required] int FormationId,
    string? Persona,
    string? SystemPrompt);

public record SessionIaTokenDto(
    string SessionToken,
    string? PersonaId,
    DateTime ExpireA);
