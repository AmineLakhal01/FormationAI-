using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FormationAI.API.Services;

public record AnamSessionResult(string SessionToken, string? PersonaId);

public interface IAnamService
{
    // Crée un session token éphémère que le client (navigateur) utilisera
    // pour établir la connexion WebRTC temps réel avec l'avatar.
    Task<AnamSessionResult> CreerSessionTokenAsync(string? persona, string? systemPrompt);
}

// Intégration de l'API anam.ai — endpoint /v1/auth/session-token
// Docs : https://docs.anam.ai
public class AnamService : IAnamService
{
    private readonly HttpClient _http;
    private readonly ILogger<AnamService> _logger;

    public AnamService(HttpClient http, IConfiguration config, ILogger<AnamService> logger)
    {
        _logger = logger;
        var section = config.GetSection("Anam");
        _http = http;
        _http.BaseAddress = new Uri(section["BaseUrl"] ?? "https://api.anam.ai");
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", section["ApiKey"] ?? "");
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<AnamSessionResult> CreerSessionTokenAsync(string? persona, string? systemPrompt)
    {
        var payload = new
        {
            personaConfig = new
            {
                name = string.IsNullOrWhiteSpace(persona) ? "Formateur IA" : persona,
                avatarId = "30fa96d0-26c4-4e55-94a0-517025942e18",
                voiceId = "6bfbe25a-979d-40f3-a92b-5394170af54b",
                llmId = "0934d97d-0c3a-4f33-91b0-5e136a0ef466",
                systemPrompt = string.IsNullOrWhiteSpace(systemPrompt)
                    ? "Tu es un formateur pédagogue qui explique les concepts clairement et répond aux questions des apprenants."
                    : systemPrompt
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _http.PostAsync("/v1/auth/session-token", content);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Erreur anam.ai session-token: {Status} {Body}", response.StatusCode, body);
            throw new HttpRequestException($"anam.ai a renvoyé {(int)response.StatusCode}");
        }

        using var doc = JsonDocument.Parse(body);
        var token = doc.RootElement.GetProperty("sessionToken").GetString()!;
        string? personaId = doc.RootElement.TryGetProperty("personaId", out var p) ? p.GetString() : null;
        return new AnamSessionResult(token, personaId);
    }
}
