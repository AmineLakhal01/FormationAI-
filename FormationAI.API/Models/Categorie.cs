using System.ComponentModel.DataAnnotations;

namespace FormationAI.API.Models;

public class Categorie
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Nom { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public ICollection<Formation> Formations { get; set; } = new List<Formation>();
}
