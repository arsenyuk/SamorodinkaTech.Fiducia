using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.MtsLink;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Клиент MTS Link (Webinar.ru) Web API v3.
/// Аутентификация: заголовок x-auth-token с API-ключом.
/// Запросы используют формат application/x-www-form-urlencoded.
/// </summary>
public class MtsLinkApiClient : IMtsLinkApiClient
{
    /// <summary>Смещение по умолчанию для времени начала встречи при невозможности парсинга (часы).</summary>
    private const int FallbackStartOffsetHours = 1;
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
    /// Создаёт экземпляр клиента MTS Link API.
    /// </summary>
    /// <param name="httpClient">HttpClient для выполнения запросов.</param>
    /// <param name="logger">Логгер.</param>
    /// <param name="baseUrl">Базовый URL MTS Link API.</param>
    /// <param name="apiToken">API-ключ (x-auth-token).</param>
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

        if (string.IsNullOrWhiteSpace(_apiToken))
            _logger.LogWarning("API-ключ MTS Link не задан — интеграция отключена");
    }

    /// <inheritdoc />
    public async Task<MtsLinkEventSession> CreateMeetingAsync(
        CreateMtsLinkMeetingRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Создание мероприятия MTS Link: {Name}", request.Name);

        // Шаг 1: Создание Event (шаблона мероприятия)
        var startDate = ParseStartTimestamp(request.StartsAtTimestamp);
        var eventForm = new Dictionary<string, string>
        {
            ["name"] = request.Name,
            ["accessSettings[isPasswordRequired]"] = "0",
            ["accessSettings[isRegistrationRequired]"] = "1",
            ["accessSettings[isModerationRequired]"] = "0",
            ["startsAt[date][year]"] = startDate.Year.ToString(),
            ["startsAt[date][month]"] = startDate.Month.ToString(),
            ["startsAt[date][day]"] = startDate.Day.ToString(),
            ["startsAt[time][hour]"] = startDate.Hour.ToString(),
            ["startsAt[time][minute]"] = startDate.Minute.ToString()
        };

        var eventResponse = await PostFormAsync("/v3/events", eventForm, cancellationToken);
        var eventJson = JsonSerializer.Deserialize<JsonElement>(eventResponse);
        var eventId = eventJson.GetProperty("eventId").GetInt32();
        var eventLink = eventJson.GetProperty("link").GetString() ?? "";

        _logger.LogDebug("Мероприятие MTS Link создано: EventId={EventId}", eventId);

        // Шаг 2: Создание EventSession из Event
        var sessionResponse = await PostFormAsync($"/v3/events/{eventId}/sessions", null, cancellationToken);
        var sessionJson = JsonSerializer.Deserialize<JsonElement>(sessionResponse);
        var sessionLink = sessionJson.TryGetProperty("link", out var linkEl) ? linkEl.GetString() ?? "" : eventLink;

        // Шаг 3: Регистрация организатора (если нужно автоматически войти)
        // Пропускается — организатор подключится вручную

        var result = new MtsLinkEventSession
        {
            Id = eventId,
            Name = request.Name,
            Status = "ACTIVE",
            StartsAt = request.StartsAtTimestamp,
            Link = sessionLink,
            Type = request.Type
        };

        _logger.LogInformation("Сессия MTS Link создана: Id={Id}, Link={Link}",
            result.Id, result.Link);

        return result;
    }

    /// <inheritdoc />
    public async Task<MtsLinkEventSession?> GetEventSessionAsync(
        int eventSessionId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Получение сессии MTS Link: EventSessionId={EventSessionId}", eventSessionId);

        // MTS Link API v3 не имеет прямого GET для сессии — возвращаем заглушку
        // В реальности данные кэшируются при создании
        _logger.LogWarning("MTS Link API v3 не предоставляет GET для сессий. " +
            "Используйте кэширование при создании.");

        return null;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteEventSessionAsync(
        int eventSessionId,
        bool sendEmail = false,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Удаление сессии MTS Link: EventSessionId={EventSessionId}, SendEmail={SendEmail}",
            eventSessionId, sendEmail);

        var url = $"{_baseUrl}/v3/events/{eventSessionId}";
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        request.Headers.Add("x-auth-token", _apiToken);

        var response = await _http.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Сессия MTS Link не найдена: EventSessionId={EventSessionId}", eventSessionId);
            return false;
        }

        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Сессия MTS Link удалена: EventSessionId={EventSessionId}", eventSessionId);
        return true;
    }

    /// <inheritdoc />
    public async Task StartEventSessionAsync(
        int eventSessionId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Запуск сессии MTS Link: EventSessionId={EventSessionId}", eventSessionId);

        var url = $"{_baseUrl}/v3/events/{eventSessionId}/start";
        var request = new HttpRequestMessage(HttpMethod.Put, url);
        request.Headers.Add("x-auth-token", _apiToken);

        var response = await _http.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("MTS Link API вернул {StatusCode}: {Body}", response.StatusCode, body);
        }

        response.EnsureSuccessStatusCode();
        _logger.LogInformation("Сессия MTS Link запущена: EventSessionId={EventSessionId}", eventSessionId);
    }

    /// <inheritdoc />
    public async Task StopEventSessionAsync(
        int eventSessionId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Завершение сессии MTS Link: EventSessionId={EventSessionId}", eventSessionId);

        var url = $"{_baseUrl}/v3/eventsessions/{eventSessionId}/stop";
        var request = new HttpRequestMessage(HttpMethod.Put, url);
        request.Headers.Add("x-auth-token", _apiToken);

        var response = await _http.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("MTS Link API вернул {StatusCode}: {Body}", response.StatusCode, body);
        }

        response.EnsureSuccessStatusCode();
        _logger.LogInformation("Сессия MTS Link завершена: EventSessionId={EventSessionId}", eventSessionId);
    }

    /// <inheritdoc />
    public async Task<MtsLinkParticipation> RegisterParticipantAsync(
        int eventSessionId,
        RegisterMtsLinkParticipantRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Регистрация участника MTS Link на сессию {EventSessionId}: {Email}",
            eventSessionId, request.Email);

        var form = new Dictionary<string, string>
        {
            ["email"] = request.Email,
            ["name"] = request.Name,
            ["secondName"] = request.SecondName,
            ["role"] = request.Role,
            ["isAutoEnter"] = request.IsAutoEnter ? "true" : "false",
            ["sendEmail"] = request.SendEmail ? "true" : "false"
        };

        var response = await PostFormAsync($"/v3/eventsessions/{eventSessionId}/register", form, cancellationToken);
        var result = JsonSerializer.Deserialize<MtsLinkParticipation>(response, JsonOptions);

        _logger.LogInformation("Участник MTS Link зарегистрирован: ParticipationId={ParticipationId}, Link={Link}",
            result?.ParticipationId, result?.Link);

        return result ?? new MtsLinkParticipation();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MtsLinkParticipation>> GetParticipationsAsync(
        int eventSessionId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Получение участников MTS Link для сессии: EventSessionId={EventSessionId}", eventSessionId);

        var url = $"{_baseUrl}/v3/eventsessions/{eventSessionId}/participations";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("x-auth-token", _apiToken);

        var response = await _http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogDebug("MTS Link API ответ GetParticipations: {ResponseBody}", responseBody);

        var json = JsonSerializer.Deserialize<JsonElement>(responseBody);

        var participations = new List<MtsLinkParticipation>();
        if (json.TryGetProperty("participations", out var arr))
        {
            foreach (var item in arr.EnumerateArray())
            {
                var p = item.Deserialize<MtsLinkParticipation>(JsonOptions);
                if (p != null)
                    participations.Add(p);
            }
        }

        return participations;
    }

    private async Task<string> PostFormAsync(
        string path,
        Dictionary<string, string>? form,
        CancellationToken cancellationToken)
    {
        var url = $"{_baseUrl}{path}";
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("x-auth-token", _apiToken);

        if (form != null)
            request.Content = new FormUrlEncodedContent(form);

        var response = await _http.SendAsync(request, cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("MTS Link API вернул {StatusCode}: {Body}", response.StatusCode, responseBody);
            response.EnsureSuccessStatusCode();
        }

        _logger.LogDebug("MTS Link API ответ {Path}: {ResponseBody}", path, responseBody);

        return responseBody;
    }

    private static DateTimeOffset ParseStartTimestamp(string timestamp)
    {
        if (DateTimeOffset.TryParse(timestamp, out var result))
            return result;

        return DateTimeOffset.UtcNow.AddHours(FallbackStartOffsetHours);
    }
}
