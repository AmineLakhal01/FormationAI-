using System.ComponentModel.DataAnnotations;

namespace FormationAI.API.DTOs;

public record ReponseCreateDto(string Texte, bool EstCorrecte);
public record QuestionCreateDto(string Enonce, List<ReponseCreateDto> Reponses);

public record QuizCreateDto(
    [Required] string Titre,
    int NoteMinimale,
    int FormationId,
    List<QuestionCreateDto> Questions);

// Soumission : pour chaque question, l'id de la réponse choisie
public record SoumissionQuizDto(int QuizId, Dictionary<int, int> Reponses);

public record ResultatQuizDto(double Note, bool Reussi, int? CertificatId, string? Reference);
