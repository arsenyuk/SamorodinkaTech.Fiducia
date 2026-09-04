using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.Edin;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Клиент ЕДИН (Mnemonios MPI) через REST API.
/// Идентификация физических лиц по ФИО + ИНН/СНИЛС/ДУЛ.
/// </summary>
public class EdinApiClient : IEdinApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<EdinApiClient> _logger;
    private readonly string _baseUrl;
    private readonly string _sourceSystemId;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Создаёт экземпляр клиента ЕДИН API.
    /// </summary>
    /// <param name="httpClient">HttpClient (BaseAddress = URL ЕДИН).</param>
    /// <param name="logger">Логгер.</param>
    /// <param name="options">Настройки ЕДИН.</param>
    public EdinApiClient(
        HttpClient httpClient,
        ILogger<EdinApiClient> logger,
        EdinOptions options)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _baseUrl = options?.BaseUrl?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(options));
        _sourceSystemId = options?.SourceSystemId ?? "fiducia";
    }

    /// <inheritdoc />
    public async Task<EdinPersonResult?> ResolvePersonAsync(
        string lastName, string firstName, string? middleName,
        string? inn, string? snils,
        string? dulType, string? dulSeries, string? dulNumber,
        CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{_baseUrl}/persons/resolve";

        _logger.LogDebug("Запрос ЕДИН resolve: {LastName} {FirstName}, ИНН={Inn}, СНИЛС={Snils}",
            lastName, firstName, inn ?? "-", snils ?? "-");

        var request = new
        {
            last_name = lastName,
            first_name = firstName,
            middle_name = middleName,
            evidence = new
            {
                inn = inn,
                snils = snils,
                dul_type = dulType,
                dul_series = dulSeries,
                dul_number = dulNumber
            },
            source_system_id = _sourceSystemId,
            external_person_id = Guid.NewGuid().ToString()
        };

        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(request, JsonOptions),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await _http.PostAsync(requestUrl, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("HTTP POST {Url} вернул {StatusCode}: {Body}",
                    requestUrl, (int)response.StatusCode, body);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<EdinResolveResponse>(
                JsonOptions, cancellationToken);

            if (result is null)
                return null;

            return new EdinPersonResult
            {
                MasterId = result.MasterId,
                Status = result.Status?.ToString() ?? "",
                HasDefects = result.HasDefects,
                Defects = result.Defects?.Select(d => d.Description ?? "").Where(d => !string.IsNullOrEmpty(d)).ToList() ?? []
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ошибка HTTP POST {Url}", requestUrl);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<EdinPersonResult?> GetPersonAsync(
        Guid masterId,
        CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{_baseUrl}/persons/{masterId}";

        _logger.LogDebug("Запрос ЕДИН get person: {MasterId}", masterId);

        try
        {
            var response = await _http.GetAsync(requestUrl, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("HTTP GET {Url} вернул {StatusCode}: {Body}",
                    requestUrl, (int)response.StatusCode, body);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<EdinPersonDto>(
                JsonOptions, cancellationToken);

            if (result is null)
                return null;

            return new EdinPersonResult
            {
                MasterId = result.MasterId,
                Status = "Matched",
                HasDefects = result.Defects?.Count > 0,
                Defects = result.Defects?.Select(d => d.Description ?? "").Where(d => !string.IsNullOrEmpty(d)).ToList() ?? []
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ошибка HTTP GET {Url}", requestUrl);
            return null;
        }
    }

    /// <summary>Внутренняя модель ответа resolve от ЕДИН API.</summary>
    private sealed record EdinResolveResponse
    {
        public string? Status { get; init; }
        public Guid? MasterId { get; init; }
        public bool HasDefects { get; init; }
        public List<EdinDefectDto>? Defects { get; init; }
    }

    /// <summary>Внутренняя модель DTO персоны от ЕДИН API.</summary>
    private sealed record EdinPersonDto
    {
        public Guid MasterId { get; init; }
        public DateTime CreatedAt { get; init; }
        public List<EdinDefectDto>? Defects { get; init; }
    }

    /// <summary>Внутренняя модель дефекта ЕДИН.</summary>
    private sealed record EdinDefectDto
    {
        public string? Description { get; init; }
    }
}
