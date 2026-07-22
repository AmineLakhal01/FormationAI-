namespace FormationAI.API.Models;

public enum UserRole
{
    Apprenant = 0,
    Formateur = 1,
    Admin = 2
}

public enum StatutSession
{
    Planifiee = 0,
    EnCours = 1,
    Terminee = 2,
    Annulee = 3
}

public enum StatutInscription
{
    EnAttente = 0,
    Confirmee = 1,
    Refusee = 2,
    Terminee = 3
}

public enum StatutVideo
{
    EnAttente = 0,
    EnCours = 1,
    Terminee = 2,
    Echec = 3
}
