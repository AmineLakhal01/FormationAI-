using System.ComponentModel.DataAnnotations;

namespace FormationAI.API.Models;

// Modalité pédagogique d'un contenu
public enum Modalite
{
    Cours = 0,
    Exercice = 1,
    Examen = 2   // Examen / Quiz
}

// Un contenu = une unité pédagogique d'un module.
// Selon la modalité : support de cours (texte / vidéo avatar), exercice, ou examen (Quiz).
public class Contenu
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Titre { get; set; } = string.Empty;

    public Modalite Modalite { get; set; } = Modalite.Cours;

    public int Ordre { get; set; }

    // Contenu textuel (cours ou énoncé d'exercice)
    [MaxLength(8000)]
    public string Corps { get; set; } = string.Empty;

    // Lien vers une vidéo de formateur IA (HeyGen) présentant le contenu
    public int? AvatarVideoId { get; set; }
    public AvatarVideo? AvatarVideo { get; set; }

    // Pour la modalité Examen : quiz associé
    public int? QuizId { get; set; }
    public Quiz? Quiz { get; set; }

    public int ModuleId { get; set; }
    public Module? Module { get; set; }
}
