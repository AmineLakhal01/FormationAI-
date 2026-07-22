using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FormationAI.API.Services;

// Résultats renvoyés au contrôleur
public record HeyGenGenerationResult(string VideoId);
public record HeyGenStatusResult(string Statut, string? VideoUrl);

public interface IHeyGenService
{
    Task<HeyGenGenerationResult> GenererVideoAsync(string script, string? avatarId, string? voiceId);
    Task<HeyGenStatusResult> ObtenirStatutAsync(string videoId);
}

// Intégration de l'API HeyGen (v2 video/generate + v1 video_status)
// Docs : https://docs.heygen.com
public class HeyGenService : IHeyGenService
{
    private readonly HttpClient _http;
    private readonly ILogger<HeyGenService> _logger;

    public HeyGenService(HttpClient http, IConfiguration config, ILogger<HeyGenService> logger)
    {
        _logger = logger;
        var section = config.GetSection("HeyGen");
        _http = http;
        _http.BaseAddress = new Uri(section["BaseUrl"] ?? "https://api.heygen.com");
        _http.DefaultRequestHeaders.Add("X-Api-Key", section["ApiKey"] ?? "");
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<HeyGenGenerationResult> GenererVideoAsync(string script, string? avatarId, string? voiceId)
    {
        var payload = new
        {
            video_inputs = new[]
            {
                new
                {
                    character = new
                    {
                        type = "avatar",
                        avatar_id = string.IsNullOrWhiteSpace(avatarId) ? "Daisy-inskirt-20220818" : avatarId,
                        avatar_style = "normal"
                    },
                    voice = new
                    {
                        type = "text",
                        input_text = script,
                        voice_id = string.IsNullOrWhiteSpace(voiceId) ? "2d5b0e6cf36f460aa7fc47e3eee4ba54" : voiceId
                    }
                }
            },
            dimension = new { width = 1280, height = 720 }
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _http.PostAsync("/v2/video/generate", content);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Erreur HeyGen generate: {Status} {Body}", response.StatusCode, body);
            throw new HttpRequestException($"HeyGen a renvoyé {(int)response.StatusCode}");
        }

        using var doc = JsonDocument.Parse(body);
        var videoId = doc.RootElement.GetProperty("data").GetProperty("video_id").GetString()!;
        return new HeyGenGenerationResult(videoId);
    }

    public async Task<HeyGenStatusResult> ObtenirStatutAsync(string videoId)
    {
        var response = await _http.GetAsync($"/v1/video_status.get?video_id={videoId}");
        var body = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(body);
        var data = doc.RootElement.GetProperty("data");
        var status = data.GetProperty("status").GetString() ?? "unknown";
        string? url = data.TryGetProperty("video_url", out var u) ? u.GetString() : null;
        return new HeyGenStatusResult(status, url);
    }
}
