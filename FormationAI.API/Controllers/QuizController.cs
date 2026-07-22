using System.Security.Claims;
using FormationAI.API.Data;
using FormationAI.API.DTOs;
using FormationAI.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FormationAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuizController : ControllerBase
{
    private readonly AppDbContext _db;
    public QuizController(AppDbContext db) => _db = db;

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    [Authorize(Roles = "Formateur,Admin")]
    public async Task<ActionResult<Quiz>> Create(QuizCreateDto dto)
    {
        var quiz = new Quiz
        {
            Titre = dto.Titre,
            NoteMinimale = dto.NoteMinimale,
            FormationId = dto.FormationId,
            Questions = dto.Questions.Select(q => new Question
            {
                Enonce = q.Enonce,
                Reponses = q.Reponses.Select(r => new Reponse
                {
                    Texte = r.Texte,
                    EstCorrecte = r.EstCorrecte
                }).ToList()
            }).ToList()
        };
        _db.Quizzes.Add(quiz);
        await _db.SaveChangesAsync();
        return Ok(new { quiz.Id, quiz.Titre });
    }

    // Version apprenant : questions et réponses SANS le champ EstCorrecte
    [HttpGet("{id:int}")]
    public async Task<ActionResult<object>> Get(int id)
    {
        var quiz = await _db.Quizzes
            .Include(q => q.Questions).ThenInclude(q => q.Reponses)
            .FirstOrDefaultAsync(q => q.Id == id);
        if (quiz is null) return NotFound();

        return Ok(new
        {
            quiz.Id,
            quiz.Titre,
            Questions = quiz.Questions.Select(q => new
            {
                q.Id,
                q.Enonce,
                Reponses = q.Reponses.Select(r => new { r.Id, r.Texte })
            })
        });
    }

    // Soumission et correction automatique ; délivre un certificat si réussi
    [HttpPost("soumettre")]
    public async Task<ActionResult<ResultatQuizDto>> Soumettre(SoumissionQuizDto dto)
    {
        var quiz = await _db.Quizzes
            .Include(q => q.Questions).ThenInclude(q => q.Reponses)
            .FirstOrDefaultAsync(q => q.Id == dto.QuizId);
        if (quiz is null) return NotFound();
        if (quiz.Questions.Count == 0) return BadRequest(new { message = "Quiz sans question." });

        int correctes = 0;
        foreach (var question in quiz.Questions)
        {
            if (dto.Reponses.TryGetValue(question.Id, out var choix))
            {
                var bonne = question.Reponses.FirstOrDefault(r => r.EstCorrecte);
                if (bonne is not null && bonne.Id == choix) correctes++;
            }
        }

        double note = Math.Round(100.0 * correctes / quiz.Questions.Count, 2);
        bool reussi = note >= quiz.NoteMinimale;

        // Mise à jour de la progression liée (si inscription existante)
        var inscription = await _db.Inscriptions
            .Include(i => i.Progression)
            .Include(i => i.SessionFormation)
            .Where(i => i.ApprenantId == UserId && i.SessionFormation!.FormationId == quiz.FormationId)
            .FirstOrDefaultAsync();
        if (inscription?.Progression is not null)
        {
            inscription.Progression.DerniereNote = note;
            if (reussi) { inscription.Progression.Termine = true; inscription.Progression.Pourcentage = 100; }
        }

        Certificat? certificat = null;
        if (reussi)
        {
            certificat = new Certificat
            {
                ApprenantId = UserId,
                FormationId = quiz.FormationId,
                Note = note
            };
            _db.Certificats.Add(certificat);
        }

        await _db.SaveChangesAsync();

        return Ok(new ResultatQuizDto(note, reussi, certificat?.Id, certificat?.Reference));
    }
}
