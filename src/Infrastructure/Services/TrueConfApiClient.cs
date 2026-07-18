using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.TrueConf;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Реализация клиента TrueConf Server API v4 через HTTP.
/// Использует OAuth2 client_credentials для авторизации.
/// </summary>
public class TrueConfApiClient : ITrueConfApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<TrueConfApiClient> _logger;
    private readonly string _serverUrl;

    private string? _accessToken;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Создаёт экземпляр клиента TrueConf API.
    /// </summary>
    /// <param name="httpClient">HttpClient для выполнения запросов.</param>
    /// <param name="logger">Логгер.</param>
    /// <param name="serverUrl">URL TrueConf Server (например, https://video.company.ru).</param>
    public TrueConfApiClient(
        HttpClient httpClient,
        ILogger<TrueConfApiClient> logger,
        string serverUrl)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serverUrl = serverUrl?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(serverUrl));
    }

    /// <inheritdoc />
    public async Task<TrueConfTokenResponse> GetTokenAsync(
        string clientId,
        string clientSecret,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Запрос OAuth2-токена TrueConf для client_id={ClientId}", clientId);

        var body = new
        {
            grant_type = "client_credentials",
            client_id = clientId,
            client_secret = clientSecret
        };

        var content = new StringContent(
            JsonSerializer.Serialize(body, JsonOptions),
            Encoding.UTF8,
            "application/json");

        var response = await _http.PostAsync(
            $"{_serverUrl}/oauth2/v1/token", content, cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<TrueConfTokenResponse>(
            JsonOptions, cancellationToken);

        _accessToken = result?.AccessToken;
        _logger.LogDebug("OAuth2-токен TrueConf получен");

        return result ?? new TrueConfTokenResponse();
    }

    /// <inheritdoc />
    public async Task<TrueConfConference> CreateConferenceAsync(
        CreateTrueConfConferenceRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Создание конференции TrueConf: {DisplayName}", request.DisplayName);

        var body = new
        {
            display_name = request.DisplayName,
            schedule = new
            {
                type = 1,
                start_time = request.StartTime,
                duration = request.Duration
            },
            tag = request.Tag
        };

        var content = new StringContent(
            JsonSerializer.Serialize(body, JsonOptions),
            Encoding.UTF8,
            "application/json");

        var url = BuildUrl("/api/v3.11/conferences");
        var response = await _http.PostAsync(url, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<TrueConfConference>(
            JsonOptions, cancellationToken);

        _logger.LogInformation("Конференция TrueConf создана: {ConferenceId} — {DisplayName}",
            result?.Id, request.DisplayName);

        return result ?? new TrueConfConference();
    }

    /// <inheritdoc />
    public async Task<TrueConfConference?> GetConferenceAsync(
        string conferenceId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Получение конференции TrueConf: {ConferenceId}", conferenceId);

        var url = BuildUrl($"/api/v3.11/conferences/{conferenceId}");
        var response = await _http.GetAsync(url, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TrueConfConference>(
            JsonOptions, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteConferenceAsync(
        string conferenceId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Удаление конференции TrueConf: {ConferenceId}", conferenceId);

        var url = BuildUrl($"/api/v3.11/conferences/{conferenceId}");
        var response = await _http.DeleteAsync(url, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return false;

        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Конференция TrueConf удалена: {ConferenceId}", conferenceId);
        return true;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TrueConfConference>> GetStoppedConferencesAsync(
        string? tag = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Получение списка завершённых конференций TrueConf (tag={Tag})", tag ?? "все");

        var url = BuildUrl("/api/v3.11/conferences?state=stopped");
        if (!string.IsNullOrEmpty(tag))
            url += $"&tag={Uri.EscapeDataString(tag)}";

        var response = await _http.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(
            cancellationToken: cancellationToken);

        var conferences = new List<TrueConfConference>();
        if (json.TryGetProperty("conferences", out var confs))
        {
            foreach (var conf in confs.EnumerateArray())
            {
                var c = JsonSerializer.Deserialize<TrueConfConference>(
                    conf.GetRawText(), JsonOptions);
                if (c != null)
                    conferences.Add(c);
            }
        }

        return conferences;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TrueConfUser>> GetUsersAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Получение списка пользователей TrueConf");

        var url = BuildUrl("/api/v3.11/users");
        var response = await _http.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(
            cancellationToken: cancellationToken);

        var users = new List<TrueConfUser>();
        if (json.TryGetProperty("users", out var usersArr))
        {
            foreach (var u in usersArr.EnumerateArray())
            {
                var user = JsonSerializer.Deserialize<TrueConfUser>(
                    u.GetRawText(), JsonOptions);
                if (user != null)
                    users.Add(user);
            }
        }

        return users;
    }

    private string BuildUrl(string path)
    {
        if (string.IsNullOrEmpty(_accessToken))
            throw new InvalidOperationException(
                "Токен доступа не получен. Вызовите GetTokenAsync перед вызовом методов API.");

        return $"{_serverUrl}{path}?access_token={Uri.EscapeDataString(_accessToken)}";
    }
}
