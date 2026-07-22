using FormationAI.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FormationAI.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Categorie> Categories => Set<Categorie>();
    public DbSet<Formation> Formations => Set<Formation>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<Contenu> Contenus => Set<Contenu>();
    public DbSet<SessionFormation> Sessions => Set<SessionFormation>();
    public DbSet<Inscription> Inscriptions => Set<Inscription>();
    public DbSet<Progression> Progressions => Set<Progression>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Reponse> Reponses => Set<Reponse>();
    public DbSet<Certificat> Certificats => Set<Certificat>();
    public DbSet<AvatarVideo> AvatarVideos => Set<AvatarVideo>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        b.Entity<Formation>()
            .HasOne(f => f.Formateur)
            .WithMany(u => u.FormationsAnimees)
            .HasForeignKey(f => f.FormateurId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Formation>()
            .HasOne(f => f.Categorie)
            .WithMany(c => c.Formations)
            .HasForeignKey(f => f.CategorieId)
            .OnDelete(DeleteBehavior.SetNull);

        b.Entity<Module>()
            .HasOne(m => m.Formation)
            .WithMany(f => f.Modules)
            .HasForeignKey(m => m.FormationId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<Contenu>()
            .HasOne(c => c.Module)
            .WithMany(m => m.Contenus)
            .HasForeignKey(c => c.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<Contenu>()
            .HasOne(c => c.Quiz)
            .WithMany()
            .HasForeignKey(c => c.QuizId)
            .OnDelete(DeleteBehavior.SetNull);

        b.Entity<Contenu>()
            .HasOne(c => c.AvatarVideo)
            .WithMany()
            .HasForeignKey(c => c.AvatarVideoId)
            .OnDelete(DeleteBehavior.SetNull);

        b.Entity<Categorie>().HasIndex(c => c.Nom).IsUnique();

        b.Entity<SessionFormation>()
            .HasOne(s => s.Formation)
            .WithMany(f => f.Sessions)
            .HasForeignKey(s => s.FormationId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<Inscription>()
            .HasOne(i => i.Apprenant)
            .WithMany(u => u.Inscriptions)
            .HasForeignKey(i => i.ApprenantId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Inscription>()
            .HasOne(i => i.SessionFormation)
            .WithMany(s => s.Inscriptions)
            .HasForeignKey(i => i.SessionFormationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Un apprenant ne s'inscrit qu'une fois par session
        b.Entity<Inscription>()
            .HasIndex(i => new { i.ApprenantId, i.SessionFormationId })
            .IsUnique();

        b.Entity<Progression>()
            .HasOne(p => p.Inscription)
            .WithOne(i => i.Progression)
            .HasForeignKey<Progression>(p => p.InscriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<Question>()
            .HasOne(q => q.Quiz)
            .WithMany(qz => qz.Questions)
            .HasForeignKey(q => q.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<Reponse>()
            .HasOne(r => r.Question)
            .WithMany(q => q.Reponses)
            .HasForeignKey(r => r.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<Formation>().Property(f => f.Prix).HasPrecision(10, 2);
    }
}
