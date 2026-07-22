namespace FormationAI.API.Models;

public class Progression
{
    public int Id { get; set; }

    public int InscriptionId { get; set; }
    public Inscription? Inscription { get; set; }

    // Pourcentage d'avancement 0-100
    public int Pourcentage { get; set; } = 0;

    // Dernière note obtenue aux quiz (0-100)
    public double? DerniereNote { get; set; }

    public bool Termine { get; set; } = false;

    public DateTime DerniereMiseAJour { get; set; } = DateTime.UtcNow;
}
