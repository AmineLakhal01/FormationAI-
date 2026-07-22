using System.Text;
using FormationAI.API.Data;
using FormationAI.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --- Connexion PostgreSQL ---
// Railway fournit DATABASE_URL au format postgres://user:pass@host:port/db
// On la convertit en chaîne Npgsql ; sinon on retombe sur appsettings.
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
var connectionString = string.IsNullOrWhiteSpace(databaseUrl)
    ? builder.Configuration.GetConnectionString("DefaultConnection")
    : ConvertirDatabaseUrl(databaseUrl);

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connectionString));

// --- Services applicatifs ---
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddHttpClient<IHeyGenService, HeyGenService>();
builder.Services.AddHttpClient<IAnamService, AnamService>();

// --- Authentification JWT ---
var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!))
        };
    });
builder.Services.AddAuthorization();

// --- CORS (front séparé) ---
builder.Services.AddCors(o => o.AddPolicy("Front", p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FormationAI API", Version = "v1" });
    var scheme = new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { scheme, Array.Empty<string>() } });
});

var app = builder.Build();

// Swagger activé aussi en prod pour la démo/examen
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("Front");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => Results.Ok(new { service = "FormationAI API", status = "ok" }));

// Migration + seed au démarrage
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(db);
}

// Railway impose le port via la variable PORT
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");

// Convertit une URL postgres://... en chaîne de connexion Npgsql
static string ConvertirDatabaseUrl(string url)
{
    var uri = new Uri(url);
    var userInfo = uri.UserInfo.Split(':');
    return $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};" +
           $"Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
}
