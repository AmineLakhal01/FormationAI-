using System.ComponentModel.DataAnnotations;

namespace FormationAI.API.DTOs;

public record FormationCreateDto(
    [Required] string Titre,
    string Description,
    int? CategorieId,
    int DureeHeures,
    decimal Prix);

public record FormationUpdateDto(
    string Titre,
    string Description,
    int? CategorieId,
    int DureeHeures,
    decimal Prix,
    bool Publiee);

public record FormationDto(
    int Id,
    string Titre,
    string Description,
    int? CategorieId,
    string? CategorieNom,
    int DureeHeures,
    decimal Prix,
    bool Publiee,
    int FormateurId,
    string FormateurNom);

// --- Catégories ---
public record CategorieDto(int Id, string Nom, string Description);
public record CategorieCreateDto([Required] string Nom, string Description);

// --- Modules & contenus ---
public record ModuleCreateDto([Required] string Titre, string Description, int Ordre, int FormationId);
public record ContenuCreateDto(
    [Required] string Titre,
    int Modalite,        // 0=Cours, 1=Exercice, 2=Examen
    int Ordre,
    string Corps,
    int? AvatarVideoId,
    int? QuizId,
    int ModuleId);

public record SessionCreateDto(
    [Required] string Titre,
    DateTime DateDebut,
    DateTime DateFin,
    int PlacesMax,
    string Lieu,
    int FormationId);

public record InscriptionCreateDto(int SessionFormationId);

public record ProgressionUpdateDto(int Pourcentage);
