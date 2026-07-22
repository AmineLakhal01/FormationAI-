using System.ComponentModel.DataAnnotations;

namespace FormationAI.API.Models;

public class Certificat
{
    public int Id { get; set; }

    [Required]
    public string Reference { get; set; } = Guid.NewGuid().ToString("N")[..12].ToUpper();

    public int ApprenantId { get; set; }
    public User? Apprenant { get; set; }

    public int FormationId { get; set; }
    public Formation? Formation { get; set; }

    public double Note { get; set; }

    public DateTime DateEmission { get; set; } = DateTime.UtcNow;
}
