using System.ComponentModel.DataAnnotations;

namespace FormationAI.API.Models;

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Nom { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Prenom { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string MotDePasseHash { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Apprenant;

    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    // Formations dont l'utilisateur est le formateur
    public ICollection<Formation> FormationsAnimees { get; set; } = new List<Formation>();

    // Inscriptions de l'apprenant
    public ICollection<Inscription> Inscriptions { get; set; } = new List<Inscription>();

    public ICollection<Certificat> Certificats { get; set; } = new List<Certificat>();
}
