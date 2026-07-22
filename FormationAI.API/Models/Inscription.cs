namespace FormationAI.API.Models;

public class Inscription
{
    public int Id { get; set; }

    public int ApprenantId { get; set; }
    public User? Apprenant { get; set; }

    public int SessionFormationId { get; set; }
    public SessionFormation? SessionFormation { get; set; }

    public DateTime DateInscription { get; set; } = DateTime.UtcNow;

    public StatutInscription Statut { get; set; } = StatutInscription.EnAttente;

    public Progression? Progression { get; set; }
}
