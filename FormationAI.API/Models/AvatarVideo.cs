using System.ComponentModel.DataAnnotations;

namespace FormationAI.API.Models;

// Vidéo de formateur IA générée via HeyGen
public class AvatarVideo
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Titre { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string ScriptTexte { get; set; } = string.Empty;

    // Identifiant renvoyé par HeyGen (video_id)
    public string? HeyGenVideoId { get; set; }

    // URL finale de la vidéo une fois générée
    public string? VideoUrl { get; set; }

    public StatutVideo Statut { get; set; } = StatutVideo.EnAttente;

    public int FormationId { get; set; }
    public Formation? Formation { get; set; }

    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
}
