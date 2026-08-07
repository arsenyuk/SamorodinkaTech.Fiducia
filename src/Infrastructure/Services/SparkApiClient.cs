using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.Spark;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Клиент СПАРК API (Интерфакс) через SOAP/XML — сервис ifaborern.asmx.
/// Аутентификация: Authmethod(Login, Password) → сессия через CookieContainer.
/// Отвечает только за SOAP-коммуникацию и управление сессией.
/// Парсинг XML-ответов делегирован в SparkXmlParser.
/// </summary>
public class SparkApiClient : ISparkApiClient, IAsyncDisposable
{
    private readonly HttpClient _http;
    private readonly ILogger<SparkApiClient> _logger;
    private readonly string _baseUrl;
    private readonly string _login;
    private readonly string _password;
    private bool _authenticated;
    private bool _disposed;

    private static readonly XNamespace SoapEnv = "http://schemas.xmlsoap.org/soap/envelope/";
    private static readonly XNamespace Tns = "http://interfax.ru/ifax";

    /// <summary>
    /// Создаёт экземпляр клиента СПАРК SOAP API.
    /// </summary>
    /// <param name="httpClient">HttpClient (должен иметь CookieContainer для сессионной аутентификации).</param>
    /// <param name="logger">Логгер.</param>
    /// <param name="baseUrl">URL SOAP-сервиса. Задаётся в конфигурации.</param>
    /// <param name="login">Логин для Authmethod.</param>
    /// <param name="password">Пароль для Authmethod.</param>
    public SparkApiClient(
        HttpClient httpClient,
        ILogger<SparkApiClient> logger,
        string baseUrl,
        string login,
        string password)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _baseUrl = baseUrl?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(baseUrl));
        _login = login ?? throw new ArgumentNullException(nameof(login));
        _password = password ?? throw new ArgumentNullException(nameof(password));

        if (string.IsNullOrWhiteSpace(_login) || string.IsNullOrWhiteSpace(_password))
            _logger.LogWarning("Логин/пароль СПАРК не заданы — интеграция отключена");
    }

    /// <inheritdoc />
    public async Task<SparkCompany?> GetCompanyByInnAsync(
        string inn,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_login))
            return null;

        _logger.LogDebug("Запрос GetCompanyShortReport из СПАРК по ИНН={Inn}", inn);

        var data = await CallSoapMethodAsync("GetCompanyShortReport",
            new XElement(Tns + "inn", inn), cancellationToken);

        var report = data?.Descendants("Report").FirstOrDefault();
        return report is null ? null : SparkXmlParser.ParseCompany(report);
    }

    /// <inheritdoc />
    public async Task<SparkManager?> GetManagerAsync(
        string inn,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_login))
            return null;

        _logger.LogDebug("Запрос руководителя из GetCompanyShortReport СПАРК по ИНН={Inn}", inn);

        var data = await CallSoapMethodAsync("GetCompanyShortReport",
            new XElement(Tns + "inn", inn), cancellationToken);

        var leader = data?.Descendants("Leader").FirstOrDefault();
        return leader is null ? null : SparkXmlParser.ParseManager(leader);
    }

    /// <inheritdoc />
    public async Task<List<SparkFounder>> GetFoundersAsync(
        string inn,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_login))
            return new List<SparkFounder>();

        _logger.LogDebug("Запрос совладельцев GetCompanyCoowners из СПАРК по ИНН={Inn}", inn);

        var data = await CallSoapMethodAsync("GetCompanyCoowners",
            new XElement(Tns + "inn", inn), cancellationToken);

        if (data?.Root is null)
            return new List<SparkFounder>();

        var coowners = data.Descendants("Coowner").ToList();
        if (coowners.Count == 0)
            coowners = data.Descendants("Owner").ToList();

        if (coowners.Count == 0)
        {
            _logger.LogWarning("GetCompanyCoowners: не удалось найти элементы Coowner/Owner в ответе для ИНН={Inn}", inn);
            return new List<SparkFounder>();
        }

        return coowners.Select(SparkXmlParser.ParseFounder)
            .Where(f => f is not null)
            .Cast<SparkFounder>()
            .ToList();
    }

    /// <summary>
    /// Закрывает SOAP-сессию вызовом End().
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed || !_authenticated)
            return;

        _disposed = true;

        try
        {
            var body = new XElement(Tns + "End");
            await SendSoapAsync(body, CancellationToken.None);
            _logger.LogDebug("SOAP-сессия СПАРК закрыта (End)");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ошибка при закрытии SOAP-сессии СПАРК (End)");
        }
    }

    // ── SOAP-коммуникация ─────────────────────────────────────────

    private async Task EnsureAuthenticatedAsync(CancellationToken ct)
    {
        if (_authenticated)
            return;

        _logger.LogDebug("Аутентификация в СПАРК: Authmethod");

        var body = new XElement(Tns + "Authmethod",
            new XElement(Tns + "Login", _login),
            new XElement(Tns + "Password", _password));

        var response = await SendSoapAsync(body, ct);
        var result = response.Descendants(Tns + "AuthmethodResult").FirstOrDefault()?.Value;

        if (string.IsNullOrEmpty(result))
            throw new InvalidOperationException("Не удалось аутентифицироваться в СПАРК: пустой ответ Authmethod");

        _authenticated = true;
        _logger.LogDebug("Аутентификация в СПАРК выполнена успешно");
    }

    /// <summary>
    /// Выполняет вызов SOAP-метода: аутентификация → запрос → извлечение xmlData.
    /// </summary>
    private async Task<XDocument?> CallSoapMethodAsync(
        string methodName,
        XElement argument,
        CancellationToken ct)
    {
        await EnsureAuthenticatedAsync(ct);

        var body = new XElement(Tns + methodName, argument);
        var response = await SendSoapAsync(body, ct);

        var xmlDataElement = response.Descendants(Tns + "xmlData").FirstOrDefault();
        if (xmlDataElement is null)
            return null;

        var xmlData = xmlDataElement.Value;
        if (string.IsNullOrWhiteSpace(xmlData))
            return null;

        try
        {
            return XDocument.Parse(xmlData);
        }
        catch
        {
            return null;
        }
    }

    private async Task<XDocument> SendSoapAsync(XElement bodyElement, CancellationToken ct)
    {
        var envelope = new XDocument(
            new XElement(SoapEnv + "Envelope",
                new XAttribute(XNamespace.Xmlns + "soap", SoapEnv),
                new XAttribute(XNamespace.Xmlns + "tns", Tns),
                new XElement(SoapEnv + "Body", bodyElement)));

        var soapXml = envelope.ToString(SaveOptions.DisableFormatting);

        var request = new HttpRequestMessage(HttpMethod.Post, _baseUrl)
        {
            Content = new StringContent(soapXml, Encoding.UTF8, "text/xml")
        };
        request.Headers.Add("SOAPAction", $"\"{Tns}{bodyElement.Name.LocalName}\"");

        var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var xml = await response.Content.ReadAsStringAsync(ct);
        return XDocument.Parse(xml);
    }
}