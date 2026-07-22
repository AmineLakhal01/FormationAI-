using System.ComponentModel.DataAnnotations;

namespace FormationAI.API.Models;

// Un module regroupe des contenus (cours, exercices, examens) au sein d'une formation
public class Module
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Titre { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    // Ordre d'affichage dans la formation
    public int Ordre { get; set; }

    public int FormationId { get; set; }
    public Formation? Formation { get; set; }

    public ICollection<Contenu> Contenus { get; set; } = new List<Contenu>();
}
