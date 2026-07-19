using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.Spark;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Реализация клиента СПАРК API (Интерфакс) через HTTP.
/// Использует API-ключ в заголовке Authorization для аутентификации.
/// </summary>
public class SparkApiClient : ISparkApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<SparkApiClient> _logger;
    private readonly string _baseUrl;
    private readonly string _apiKey;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Создаёт экземпляр клиента СПАРК API.
    /// </summary>
    /// <param name="httpClient">HttpClient для выполнения запросов.</param>
    /// <param name="logger">Логгер.</param>
    /// <param name="baseUrl">Базовый URL API СПАРК (например, https://api.spark-interfax.ru).</param>
    /// <param name="apiKey">API-ключ для аутентификации.</param>
    public SparkApiClient(
        HttpClient httpClient,
        ILogger<SparkApiClient> logger,
        string baseUrl,
        string apiKey)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _baseUrl = baseUrl?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(baseUrl));
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));

        if (string.IsNullOrWhiteSpace(_apiKey))
            _logger.LogWarning("API-ключ СПАРК не задан — интеграция отключена");
    }

    /// <inheritdoc />
    public async Task<SparkCompany?> GetCompanyByInnAsync(
        string inn,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            return null;

        _logger.LogDebug("Запрос карточки компании из СПАРК по ИНН={Inn}", inn);

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"{_baseUrl}/api/v1/companies/{inn}");
        request.Headers.Add("Authorization", $"Bearer {_apiKey}");

        var response = await _http.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var company = await response.Content.ReadFromJsonAsync<SparkCompany>(JsonOptions, cancellationToken);
        return company;
    }

    /// <inheritdoc />
    public async Task<SparkManager?> GetManagerAsync(
        string inn,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            return null;

        _logger.LogDebug("Запрос данных о гендиректоре из СПАРК по ИНН={Inn}", inn);

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"{_baseUrl}/api/v1/companies/{inn}/managers");
        request.Headers.Add("Authorization", $"Bearer {_apiKey}");

        var response = await _http.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var managers = await response.Content
            .ReadFromJsonAsync<List<SparkManager>>(JsonOptions, cancellationToken);

        return managers?.FirstOrDefault();
    }
}
