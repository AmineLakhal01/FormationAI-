using System.ComponentModel.DataAnnotations;

namespace FormationAI.API.Models;

public class Formation
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Titre { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    // Catégorie (relation)
    public int? CategorieId { get; set; }
    public Categorie? Categorie { get; set; }

    // Durée estimée en heures
    public int DureeHeures { get; set; }

    public decimal Prix { get; set; }

    public bool Publiee { get; set; } = false;

    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    // Formateur responsable
    public int FormateurId { get; set; }
    public User? Formateur { get; set; }

    public ICollection<Module> Modules { get; set; } = new List<Module>();
    public ICollection<SessionFormation> Sessions { get; set; } = new List<SessionFormation>();
    public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
    public ICollection<AvatarVideo> AvatarVideos { get; set; } = new List<AvatarVideo>();
}
