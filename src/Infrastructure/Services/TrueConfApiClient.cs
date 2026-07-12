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
            type = 0,
            topic = request.Topic,
            call_type = "video",
            access_mode = request.AccessMode,
            owner = request.Owner,
            auto_invite = 1,
            invitations = request.Invitations.Select(i => new { id = i.Id, role = i.Role }),
            schedule = new
            {
                type = 1,
                start_time = request.StartTime,
                duration = request.Duration
            },
            tag = request.Tag
        };

        var jsonBody = JsonSerializer.Serialize(body, JsonOptions);
        var url = BuildUrl("/api/v3.11/conferences");
        _logger.LogDebug("TrueConf API CreateConference: {Url} | {RequestBody}", url, jsonBody);

        var content = new StringContent(
            jsonBody,
            Encoding.UTF8,
            "application/json");

        var response = await _http.PostAsync(url, content, cancellationToken);

        var responsebody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("TrueConf API вернул {StatusCode}: {ErrorBody}", response.StatusCode, responsebody);
            response.EnsureSuccessStatusCode();
        }

        _logger.LogDebug("TrueConf API ответ при создании конференции: {ResponseBody}", responsebody);

        // Ответ вложен в { "conference": { ... } }
        var json = JsonSerializer.Deserialize<JsonElement>(responsebody);
        var conferenceElement = json.TryGetProperty("conference", out var conf) ? conf : json;

        var result = conferenceElement.Deserialize<TrueConfConference>(JsonOptions);

        if (result != null && string.IsNullOrEmpty(result.JoinLink))
        {
            // TrueConf API не возвращает join_link — собираем URL вручную
            // Формат прямой ссылки на конференцию: {BaseUrl}/c/{Id}
            var baseUrl = _serverUrl.Replace("https://", "http://");
            result = new TrueConfConference
            {
                Id = result.Id,
                DisplayName = result.DisplayName,
                State = result.State,
                Schedule = result.Schedule,
                JoinLink = $"{baseUrl}/c/{result.Id}"
            };
        }

        _logger.LogInformation("Конференция TrueConf создана: {ConferenceId} — {DisplayName}, JoinLink: {JoinLink}",
            result?.Id, request.DisplayName, result?.JoinLink);

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

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogDebug("TrueConf API ответ GetConference: {ResponseBody}", responseBody);

        response.EnsureSuccessStatusCode();

        // Ответ может быть вложен в { "conference": { ... } }
        var json = JsonSerializer.Deserialize<JsonElement>(responseBody);
        var conferenceElement = json.TryGetProperty("conference", out var conf) ? conf : json;

        var result = conferenceElement.Deserialize<TrueConfConference>(JsonOptions);

        // Собираем JoinLink если не вернулся
        if (result != null && string.IsNullOrEmpty(result.JoinLink))
        {
            var baseUrl = _serverUrl.Replace("https://", "http://");
            result = new TrueConfConference
            {
                Id = result.Id,
                DisplayName = result.DisplayName,
                State = result.State,
                Schedule = result.Schedule,
                JoinLink = $"{baseUrl}/c/{result.Id}"
            };
        }

        return result;
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

    /// <inheritdoc />
    public async Task<TrueConfServerInfo?> GetServerInfoAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Получение информации о сервере TrueConf");

        var url = BuildUrl("/api/v4/server");
        var response = await _http.GetAsync(url, cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogDebug("TrueConf API ответ GetServerInfo: {ResponseBody}", responseBody);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("TrueConf API вернул {StatusCode}: {ErrorBody}", response.StatusCode, responseBody);
            return null;
        }

        var json = JsonSerializer.Deserialize<JsonElement>(responseBody);

        if (!json.TryGetProperty("product", out var product))
            return null;

        string docUrl = "";
        if (json.TryGetProperty("links", out var links) && links.TryGetProperty("documentation", out var doc))
            docUrl = doc.GetString() ?? "";

        string siteUrl = "";
        if (json.TryGetProperty("links", out var links2) && links2.TryGetProperty("site_url", out var site))
            siteUrl = site.GetString() ?? "";

        string webUrl = "";
        if (json.TryGetProperty("web_config", out var wc) && wc.TryGetProperty("url", out var wu))
            webUrl = wu.GetString() ?? "";

        return new TrueConfServerInfo
        {
            DisplayName = product.TryGetProperty("display_name", out var dn) ? dn.GetString() ?? "" : "",
            Id = product.TryGetProperty("id", out var id) ? id.GetString() ?? "" : "",
            Name = product.TryGetProperty("name", out var name) ? name.GetString() ?? "" : "",
            Version = product.TryGetProperty("version", out var ver) ? ver.GetString() ?? "" : "",
            Platform = product.TryGetProperty("platform", out var plat) ? plat.GetString() ?? "" : "",
            DocumentationUrl = docUrl,
            SiteUrl = siteUrl,
            WebConfigUrl = webUrl
        };
    }

    private string BuildUrl(string path)
    {
        if (string.IsNullOrEmpty(_accessToken))
            throw new InvalidOperationException(
                "Токен доступа не получен. Вызовите GetTokenAsync перед вызовом методов API.");

        return $"{_serverUrl}{path}?access_token={Uri.EscapeDataString(_accessToken)}";
    }
}
