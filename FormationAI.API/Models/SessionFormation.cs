using System.ComponentModel.DataAnnotations;

namespace FormationAI.API.Models;

// Nommée SessionFormation pour éviter la collision avec HttpContext.Session
public class SessionFormation
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Titre { get; set; } = string.Empty;

    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }

    public int PlacesMax { get; set; } = 20;

    [MaxLength(200)]
    public string Lieu { get; set; } = "En ligne";

    public StatutSession Statut { get; set; } = StatutSession.Planifiee;

    public int FormationId { get; set; }
    public Formation? Formation { get; set; }

    public ICollection<Inscription> Inscriptions { get; set; } = new List<Inscription>();
}
