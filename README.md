# FormationAI — Gestion des formations avec formateur IA

Application web de gestion des formations. Backend **ASP.NET Core (.NET 10)** exposant une API REST, et front web statique. Un **formateur IA** (avatar) est intégré via **HeyGen** (vidéos de présentation) et **anam.ai** (avatar conversationnel temps réel, principalement audio).

Deux parties : `FormationAI.API/` (backend) et `frontend/` (HTML/CSS/JS — voir `frontend/README.md`).

## Fonctionnalités

- Authentification / inscription : JWT, mots de passe hachés (BCrypt), 3 rôles (Apprenant, Formateur, Admin).
- Interface d'administration : utilisateurs, formateurs, statistiques.
- Formations, catégories, formateurs, modules et contenus.
- Modalités pédagogiques : Cours, Exercice, Examen (Quiz).
- Inscription des apprenants, progression, quiz auto-corrigés, certificats vérifiables.
- Formateur IA : génération de vidéos (HeyGen) et session temps réel (anam.ai).

## Stack

.NET 10 · ASP.NET Core Web API · Entity Framework Core · PostgreSQL · JWT · Swagger · Docker · Railway.

> Base : « SQL Server ou équivalent » → PostgreSQL (natif Railway). Pour SQL Server, remplacer le package `Npgsql.EntityFrameworkCore.PostgreSQL` par `Microsoft.EntityFrameworkCore.SqlServer` et `UseNpgsql` par `UseSqlServer` dans `Program.cs`.

## Lancer en local

Prérequis : SDK .NET 10, Docker (pour PostgreSQL).

```bash
# 1) Base de données
docker run --name formationai-db -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=formationai -p 5432:5432 -d postgres:16

# 2) API
cd FormationAI.API
dotnet restore
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate
dotnet run
```

Swagger : http://localhost:8080/swagger

Comptes de démo : `admin@formationai.dev` / `Admin@123` — `formateur@formationai.dev` / `Formateur@123`

Front : servir le dossier `frontend/` (VS Code → Live Server, ou `python -m http.server 5500 --directory frontend`).

## Déploiement Railway

1. Pousser sur GitHub.
2. Railway → Deploy from GitHub repo.
3. Ajouter un service PostgreSQL (crée `DATABASE_URL`, lu automatiquement).
4. Définir les variables `Jwt__Key`, `HeyGen__ApiKey`, `Anam__ApiKey` (voir `.env.example`).
5. Railway détecte le `Dockerfile` et déploie ; le port vient de `PORT`.
