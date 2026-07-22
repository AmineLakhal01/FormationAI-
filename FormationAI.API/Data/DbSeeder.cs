using FormationAI.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FormationAI.API.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.MigrateAsync();

        if (await db.Users.AnyAsync()) return;

        var admin = new User
        {
            Nom = "Admin",
            Prenom = "Système",
            Email = "admin@formationai.dev",
            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = UserRole.Admin
        };

        var formateur = new User
        {
            Nom = "Lakhal",
            Prenom = "Mohamed Amine",
            Email = "formateur@formationai.dev",
            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Formateur@123"),
            Role = UserRole.Formateur
        };

        db.Users.AddRange(admin, formateur);
        await db.SaveChangesAsync();

        var categorie = new Categorie { Nom = "Développement", Description = "Programmation et génie logiciel" };
        db.Categories.Add(categorie);
        await db.SaveChangesAsync();

        var formation = new Formation
        {
            Titre = "Introduction au développement .NET",
            Description = "Formation complète sur ASP.NET Core, animée par un formateur IA.",
            CategorieId = categorie.Id,
            DureeHeures = 20,
            Prix = 299.99m,
            Publiee = true,
            FormateurId = formateur.Id
        };
        db.Formations.Add(formation);
        await db.SaveChangesAsync();

        var module = new Module
        {
            Titre = "Module 1 — Les fondamentaux",
            Description = "Découverte de l'écosystème .NET",
            Ordre = 1,
            FormationId = formation.Id
        };
        db.Modules.Add(module);
        await db.SaveChangesAsync();

        db.Contenus.Add(new Contenu
        {
            Titre = "Présentation du module par le formateur IA",
            Modalite = Modalite.Cours,
            Ordre = 1,
            Corps = "Bienvenue dans ce module d'introduction à .NET.",
            ModuleId = module.Id
        });
        await db.SaveChangesAsync();

        db.Sessions.Add(new SessionFormation
        {
            Titre = "Session de septembre",
            DateDebut = DateTime.UtcNow.AddDays(7),
            DateFin = DateTime.UtcNow.AddDays(14),
            PlacesMax = 25,
            FormationId = formation.Id
        });
        await db.SaveChangesAsync();
    }
}
