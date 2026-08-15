using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.CbrFinOrg;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Клиент SOAP-сервиса ЦБ РФ (FinOrg.asmx) — HTTP POST с application/x-www-form-urlencoded.
/// Протокол отличается от SPARK: нет SOAP-envelope, нет аутентификации.
/// Ответ — XML, парсинг делегирован в CbrFinOrgXmlParser.
/// </summary>
public class CbrFinOrgApiClient : ICbrFinOrgClient
{
    private readonly HttpClient _http;
    private readonly ILogger<CbrFinOrgApiClient> _logger;
    private readonly string _baseUrl;

    /// <summary>
    /// Создаёт экземпляр клиента FinOrg API.
    /// </summary>
    /// <param name="httpClient">HttpClient (stateless, без CookieContainer).</param>
    /// <param name="logger">Логгер.</param>
    /// <param name="baseUrl">URL сервиса FinOrg (по умолчанию https://cbr.ru/FO_ZoomWS/FinOrg.asmx).</param>
    public CbrFinOrgApiClient(
        HttpClient httpClient,
        ILogger<CbrFinOrgApiClient> logger,
        string baseUrl)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _baseUrl = baseUrl?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(baseUrl));
    }

    /// <inheritdoc />
    public async Task<CbrFinOrgOrganization?> GetOrganizationByInnAsync(
        long inn,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Запрос FinOrg GetFullInfoByINN по ИНН={Inn}", inn);

        var response = await PostFormAsync("GetFullInfoByINN",
            new Dictionary<string, string> { ["INN"] = inn.ToString() },
            cancellationToken);

        if (response is null)
            return null;

        var resultElement = response.Element("GetFullInfoByINNResponse")?.Element("GetFullInfoByINNResult");
        return CbrFinOrgXmlParser.ParseFullInfo(resultElement);
    }

    /// <inheritdoc />
    public async Task<CbrFinOrgOrganization?> GetOrganizationByOgrnAsync(
        long ogrn,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Запрос FinOrg GetFullInfoByOGRN по ОГРН={Ogrn}", ogrn);

        var response = await PostFormAsync("GetFullInfoByOGRN",
            new Dictionary<string, string> { ["OGRN"] = ogrn.ToString() },
            cancellationToken);

        if (response is null)
            return null;

        var resultElement = response.Element("GetFullInfoByOGRNResponse")?.Element("GetFullInfoByOGRNResult");
        return CbrFinOrgXmlParser.ParseFullInfo(resultElement);
    }

    /// <inheritdoc />
    public async Task<CbrFinOrgSearchResult> SearchAsync(
        string? name,
        string? address,
        int page = 0,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Запрос FinOrg Search: Name={Name}, Addr={Addr}, Page={Page}",
            name ?? "*", address ?? "*", page);

        var parameters = new Dictionary<string, string>
        {
            ["Status"] = "Active",
            ["VidID"] = "-1",
            ["OKATO"] = "-1",
            ["page"] = page.ToString()
        };

        if (!string.IsNullOrWhiteSpace(name))
            parameters["Name"] = name;
        if (!string.IsNullOrWhiteSpace(address))
            parameters["Addr"] = address;

        var response = await PostFormAsync("Search", parameters, cancellationToken);

        if (response is null)
            return new CbrFinOrgSearchResult();

        var resultElement = response.Element("SearchResponse")?.Element("SearchResult");
        return CbrFinOrgXmlParser.ParseSearchResult(resultElement);
    }

    /// <inheritdoc />
    public async Task<List<CbrFinOrgRecord>> SearchByInnsAsync(
        long[] inns,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Запрос FinOrg SearchByINNs: {Count} ИНН", inns.Length);

        var response = await PostFormAsync("SearchByINNs",
            new Dictionary<string, string> { ["INNs"] = string.Join(",", inns) },
            cancellationToken);

        if (response is null)
            return new List<CbrFinOrgRecord>();

        var resultElement = response.Element("SearchByINNsResponse")?.Element("SearchByINNsResult");
        var dsElement = resultElement?.Element("DS");
        if (dsElement is null)
            return new List<CbrFinOrgRecord>();

        return dsElement.Elements("Record")
            .Select(CbrFinOrgXmlParser.ParseRecord)
            .Where(r => r is not null)
            .Cast<CbrFinOrgRecord>()
            .ToList();
    }

    /// <inheritdoc />
    public async Task<DateTime> GetLastUpdateAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Запрос FinOrg GetLastUpdate");

        var response = await PostFormAsync("GetLastUpdate",
            new Dictionary<string, string>(),
            cancellationToken);

        if (response is null)
            return default;

        var resultElement = response.Element("GetLastUpdateResponse")?.Element("GetLastUpdateResult");
        return DateTime.TryParse(resultElement?.Value, out var date) ? date : default;
    }

    // ── HTTP-коммуникация ─────────────────────────────────────────

    private async Task<XDocument?> PostFormAsync(
        string methodName,
        Dictionary<string, string> parameters,
        CancellationToken ct)
    {
        var url = $"{_baseUrl}/{methodName}";

        try
        {
            using var content = new FormUrlEncodedContent(parameters);
            var response = await _http.PostAsync(url, content, ct);
            response.EnsureSuccessStatusCode();

            var xml = await response.Content.ReadAsStringAsync(ct);
            return XDocument.Parse(xml);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Ошибка HTTP при вызове FinOrg метода {Method}: {Message}",
                methodName, ex.Message);
            throw;
        }
    }
}
