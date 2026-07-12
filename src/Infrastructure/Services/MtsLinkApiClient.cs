using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.MtsLink;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Реализация клиента API МТС Линк v3.
/// Использует авторизацию через x-auth-token в заголовке запроса.
/// Создание встречи выполняется в два шага: Event (шаблон) → EventSession (встреча).
/// </summary>
public class MtsLinkApiClient : IMtsLinkApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<MtsLinkApiClient> _logger;
    private readonly string _baseUrl;
    private readonly string _apiToken;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Создаёт экземпляр клиента МТС Линк API.
    /// </summary>
    /// <param name="httpClient">HttpClient для запросов.</param>
    /// <param name="logger">Логгер.</param>
    /// <param name="baseUrl">Базовый URL API (по умолчанию https://userapi.mts-link.ru).</param>
    /// <param name="apiToken">API-ключ для авторизации.</param>
    public MtsLinkApiClient(
        HttpClient httpClient,
        ILogger<MtsLinkApiClient> logger,
        string baseUrl,
        string apiToken)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _baseUrl = baseUrl?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(baseUrl));
        _apiToken = apiToken ?? throw new ArgumentNullException(nameof(apiToken));
    }

    /// <inheritdoc />
    public async Task<MtsLinkEventSession> CreateMeetingAsync(
        CreateMtsLinkMeetingRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Создание встречи МТС Линк: {Name}", request.Name);

        // Шаг 1: создать шаблон Event
        var eventData = new Dictionary<string, string>
        {
            ["name"] = request.Name,
            ["accessSettings[isPasswordRequired]"] = "0",
            ["accessSettings[isModerationRequired]"] = "0",
            ["accessSettings[isRegistrationRequired]"] = "0",
            ["type"] = request.Type,
            ["lang"] = request.Lang
        };

        if (!string.IsNullOrEmpty(request.Description))
            eventData["description"] = request.Description;

        if (!string.IsNullOrEmpty(request.Duration))
            eventData["duration"] = request.Duration;

        var eventResponse = await PostFormAsync<MtsLinkEvent>(
            "/v3/events", eventData, cancellationToken);

        _logger.LogDebug("Шаблон Event создан: eventId={EventId}", eventResponse.EventId);

        // Шаг 2: создать EventSession
        var sessionData = new Dictionary<string, string>
        {
            ["name"] = request.Name
        };

        if (!string.IsNullOrEmpty(request.StartsAtTimestamp))
            sessionData["startsAtTimestamp"] = request.StartsAtTimestamp;

        var sessionResponse = await PostFormAsync<MtsLinkEventSession>(
            $"/v3/events/{eventResponse.EventId}/sessions", sessionData, cancellationToken);

        _logger.LogInformation(
            "Встреча МТС Линк создана: eventSessionId={Id} link={Link}",
            sessionResponse.Id, sessionResponse.Link);

        return sessionResponse;
    }

    /// <inheritdoc />
    public async Task<MtsLinkEventSession?> GetEventSessionAsync(
        int eventSessionId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Получение мероприятия МТС Линк: eventSessionId={Id}", eventSessionId);

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"{_baseUrl}/v3/eventsessions/{eventSessionId}");
        request.Headers.Add("x-auth-token", _apiToken);

        var response = await _http.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<MtsLinkEventSession>(
            JsonOptions, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteEventSessionAsync(
        int eventSessionId,
        bool sendEmail = false,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Удаление мероприятия МТС Линк: eventSessionId={Id}", eventSessionId);

        var request = new HttpRequestMessage(HttpMethod.Delete,
            $"{_baseUrl}/v3/eventsessions/{eventSessionId}");
        request.Headers.Add("x-auth-token", _apiToken);

        if (!sendEmail)
        {
            request.Content = new FormUrlEncodedContent(
                new Dictionary<string, string> { ["sendEmail"] = "false" });
        }

        var response = await _http.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return false;

        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Мероприятие МТС Линк удалено: eventSessionId={Id}", eventSessionId);
        return true;
    }

    /// <inheritdoc />
    public async Task StartEventSessionAsync(
        int eventSessionId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Запуск мероприятия МТС Линк: eventSessionId={Id}", eventSessionId);

        await PutAsync($"/v3/eventsessions/{eventSessionId}/start", cancellationToken);

        _logger.LogInformation("Мероприятие МТС Линк запущено: eventSessionId={Id}", eventSessionId);
    }

    /// <inheritdoc />
    public async Task StopEventSessionAsync(
        int eventSessionId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Остановка мероприятия МТС Линк: eventSessionId={Id}", eventSessionId);

        await PutAsync($"/v3/eventsessions/{eventSessionId}/stop", cancellationToken);

        _logger.LogInformation("Мероприятие МТС Линк остановлено: eventSessionId={Id}", eventSessionId);
    }

    /// <inheritdoc />
    public async Task<MtsLinkParticipation> RegisterParticipantAsync(
        int eventSessionId,
        RegisterMtsLinkParticipantRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Регистрация участника в МТС Линк: {Email} на eventSessionId={Id}",
            request.Email, eventSessionId);

        var data = new Dictionary<string, string>
        {
            ["email"] = request.Email,
            ["role"] = request.Role,
            ["sendEmail"] = request.SendEmail ? "true" : "false"
        };

        if (!string.IsNullOrEmpty(request.Name))
            data["name"] = request.Name;
        if (!string.IsNullOrEmpty(request.SecondName))
            data["secondName"] = request.SecondName;

        var result = await PostFormAsync<MtsLinkParticipation>(
            $"/v3/eventsessions/{eventSessionId}/register", data, cancellationToken);

        _logger.LogDebug("Участник зарегистрирован: participationId={ParticipationId}",
            result.ParticipationId);

        return result;
    }

    private async Task<T> PostFormAsync<T>(
        string path, Dictionary<string, string> data, CancellationToken ct)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}{path}");
        request.Headers.Add("x-auth-token", _apiToken);
        request.Content = new FormUrlEncodedContent(data);

        var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct)
               ?? throw new InvalidOperationException("Пустой ответ API МТС Линк");
    }

    private async Task PutAsync(string path, CancellationToken ct)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"{_baseUrl}{path}");
        request.Headers.Add("x-auth-token", _apiToken);

        var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
    }
}
