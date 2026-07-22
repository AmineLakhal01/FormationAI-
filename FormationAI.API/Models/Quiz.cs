using System.ComponentModel.DataAnnotations;

namespace FormationAI.API.Models;

public class Quiz
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Titre { get; set; } = string.Empty;

    public int NoteMinimale { get; set; } = 50; // seuil de réussite en %

    public int FormationId { get; set; }
    public Formation? Formation { get; set; }

    public ICollection<Question> Questions { get; set; } = new List<Question>();
}

public class Question
{
    public int Id { get; set; }

    [Required, MaxLength(500)]
    public string Enonce { get; set; } = string.Empty;

    public int QuizId { get; set; }
    public Quiz? Quiz { get; set; }

    public ICollection<Reponse> Reponses { get; set; } = new List<Reponse>();
}

public class Reponse
{
    public int Id { get; set; }

    [Required, MaxLength(300)]
    public string Texte { get; set; } = string.Empty;

    public bool EstCorrecte { get; set; }

    public int QuestionId { get; set; }
    public Question? Question { get; set; }
}
