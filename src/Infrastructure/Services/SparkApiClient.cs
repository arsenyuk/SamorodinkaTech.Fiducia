using System.Globalization;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.Spark;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Клиент СПАРК API (Интерфакс) через SOAP/XML — сервис ifaborern.asmx.
/// Аутентификация: Authmethod(Login, Password) → сессия через CookieContainer.
/// Сессия закрывается вызовом End() при Dispose.
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
    /// <param name="baseUrl">URL SOAP-сервиса (например, http://sparkgatetest.interfax.ru/iFaxWebService/ifaborern.asmx).</param>
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

        await EnsureAuthenticatedAsync(cancellationToken);

        _logger.LogDebug("Запрос GetCompanyShortReport из СПАРК по ИНН={Inn}", inn);

        var body = new XElement(Tns + "GetCompanyShortReport",
            new XElement(Tns + "inn", inn));

        var response = await SendSoapAsync(body, cancellationToken);
        var data = GetXmlData(response);

        var report = data?.Descendants("Report").FirstOrDefault();
        if (report is null)
            return null;

        return ParseCompany(report);
    }

    /// <inheritdoc />
    public async Task<SparkManager?> GetManagerAsync(
        string inn,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_login))
            return null;

        await EnsureAuthenticatedAsync(cancellationToken);

        _logger.LogDebug("Запрос руководителя из GetCompanyShortReport СПАРК по ИНН={Inn}", inn);

        var body = new XElement(Tns + "GetCompanyShortReport",
            new XElement(Tns + "inn", inn));

        var response = await SendSoapAsync(body, cancellationToken);
        var data = GetXmlData(response);

        var leader = data?.Descendants("Leader").FirstOrDefault();
        if (leader is null)
            return null;

        return ParseManager(leader);
    }

    /// <inheritdoc />
    public async Task<List<SparkFounder>> GetFoundersAsync(
        string inn,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_login))
            return new List<SparkFounder>();

        await EnsureAuthenticatedAsync(cancellationToken);

        _logger.LogDebug("Запрос совладельцев GetCompanyCoowners из СПАРК по ИНН={Inn}", inn);

        var body = new XElement(Tns + "GetCompanyCoowners",
            new XElement(Tns + "inn", inn));

        var response = await SendSoapAsync(body, cancellationToken);
        var data = GetXmlData(response);

        if (data?.Root is null)
            return new List<SparkFounder>();

        // Структура ответа GetCompanyCoowners неизвестна — реализован best-effort парсинг.
        // Пытаемся распарсить по известной структуре: Data > Coowner (или Owner).
        var coowners = data.Descendants("Coowner").ToList();
        if (coowners.Count == 0)
        {
            coowners = data.Descendants("Owner").ToList();
            if (coowners.Count == 0)
            {
                _logger.LogWarning("GetCompanyCoowners: не удалось найти элементы Coowner/Owner в ответе для ИНН={Inn}", inn);
                return new List<SparkFounder>();
            }
        }

        return coowners.Select(ParseFounder).Where(f => f is not null).Cast<SparkFounder>().ToList();
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

    // ── Приватные методы ──────────────────────────────────────────

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

    /// <summary>
    /// Извлекает xmlData из SOAP-ответа и парсит как отдельный XDocument.
    /// </summary>
    private static XDocument? GetXmlData(XDocument soapResponse)
    {
        var xmlDataElement = soapResponse.Descendants(Tns + "xmlData").FirstOrDefault();
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

    // ── Парсеры ───────────────────────────────────────────────────

    private static SparkCompany ParseCompany(XElement report)
    {
        var okopf = report.Element("OKOPF");
        var status = report.Element("Status");

        return new SparkCompany
        {
            SparkId = ParseInt(report.Element("SparkID")?.Value) ?? 0,
            CompanyType = ParseInt(report.Element("CompanyType")?.Value) ?? 1,
            Inn = report.Element("INN")?.Value ?? "",
            Kpp = report.Element("KPP")?.Value,
            Ogrn = report.Element("OGRN")?.Value,
            Okpo = report.Element("OKPO")?.Value,
            FullName = report.Element("FullNameRus")?.Value ?? "",
            ShortName = report.Element("ShortNameRus")?.Value,
            OkopfCode = okopf?.Attribute("Code")?.Value,
            OkopfName = okopf?.Attribute("Name")?.Value,
            LegalAddress = report.Element("LegalAddresses")
                ?.Element("Address")
                ?.Attribute("Address")?.Value,
            IsActing = status?.Attribute("IsActing")?.Value == "true",
            Status = status?.Attribute("Type")?.Value,
            RegistrationDate = ParseDate(report.Element("DateFirstReg")?.Value),
            CharterCapital = ParseDecimal(report.Element("CharterCapital")?.Value)
        };
    }

    private static SparkManager ParseManager(XElement leader)
    {
        return new SparkManager
        {
            FullName = leader.Attribute("FIO")?.Value ?? "",
            Position = leader.Attribute("Position")?.Value,
            Inn = leader.Attribute("INN")?.Value,
            ActualDate = ParseDate(leader.Attribute("ActualDate")?.Value),
            LegalCapacityEndDate = ParseDate(leader.Attribute("LegalCapacityEndDate")?.Value),
            ManagementCompany = leader.Attribute("ManagementCompany")?.Value,
            ManagementCompanyINN = leader.Attribute("ManagementCompanyINN")?.Value
        };
    }

    /// <summary>
    /// Парсит элемент Coowner/Owner из GetCompanyCoowners.
    /// Структура точно не известна — реализован best-effort парсинг.
    /// </summary>
    private static SparkFounder? ParseFounder(XElement coowner)
    {
        var type = ParseInt(coowner.Attribute("Type")?.Value ?? coowner.Element("Type")?.Value);
        var name = coowner.Attribute("Name")?.Value ?? coowner.Element("Name")?.Value;
        var inn = coowner.Attribute("INN")?.Value ?? coowner.Element("INN")?.Value;
        var ogrn = coowner.Attribute("OGRN")?.Value ?? coowner.Element("OGRN")?.Value;
        var fullName = coowner.Attribute("FullName")?.Value ?? coowner.Element("FullName")?.Value;
        var personInn = coowner.Attribute("PersonINN")?.Value ?? coowner.Element("PersonINN")?.Value;
        var shareAmount = ParseDecimal(coowner.Attribute("ShareAmount")?.Value ?? coowner.Element("ShareAmount")?.Value);
        var sharePercent = ParseDecimal(coowner.Attribute("SharePercent")?.Value ?? coowner.Element("SharePercent")?.Value);
        var country = coowner.Attribute("Country")?.Value ?? coowner.Element("Country")?.Value;
        var entryDate = ParseDate(coowner.Attribute("EntryDate")?.Value ?? coowner.Element("EntryDate")?.Value);
        var exitDate = ParseDate(coowner.Attribute("ExitDate")?.Value ?? coowner.Element("ExitDate")?.Value);
        var citizenship = coowner.Attribute("Citizenship")?.Value ?? coowner.Element("Citizenship")?.Value;

        // CoownerType: 0=российское ЮЛ, 1=иностранное ЮЛ, 2=ФЛ
        var isForeign = type == 1;

        return new SparkFounder
        {
            Name = name,
            Inn = inn,
            Ogrn = ogrn,
            Country = country ?? (isForeign ? "Иностранное" : null),
            IsForeign = isForeign,
            FullName = fullName,
            PersonInn = personInn,
            Citizenship = citizenship,
            ShareAmount = shareAmount,
            SharePercent = sharePercent,
            EntryDate = entryDate,
            ExitDate = exitDate
        };
    }

    // ── Вспомогательные методы парсинга ───────────────────────────

    private static int? ParseInt(string? value)
    {
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : null;
    }

    private static DateTime? ParseDate(string? value)
    {
        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result) ? result : null;
    }

    private static decimal? ParseDecimal(string? value)
    {
        return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : null;
    }
}