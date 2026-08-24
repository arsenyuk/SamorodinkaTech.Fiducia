using System.Globalization;
using System.Web;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Interfaces;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Парсер декодированного текста QR-кода нотариального документа.
/// QR-код通常 содержит URL вида:
///   https://reestr.notariat.ru/ru/notarial-acts/?id=...&date=...&notary=...
/// или прямые данные в формате key=value;...
/// </summary>
public sealed class NotarizationQrParser : INotarizationQrParser
{
    private readonly ILogger<NotarizationQrParser> _logger;

    private static readonly HashSet<string> _notarialDomains = new(StringComparer.OrdinalIgnoreCase)
    {
        "reestr.notariat.ru",
        "notariat.ru",
        "notarialact.ru"
    };

    public NotarizationQrParser(ILogger<NotarizationQrParser> logger)
    {
        _logger = logger;
    }

    public NotarizationQrData? Parse(string qrText)
    {
        if (string.IsNullOrWhiteSpace(qrText))
            return null;

        qrText = qrText.Trim();

        // Попытка 1: URL с query-параметрами
        if (Uri.TryCreate(qrText, UriKind.Absolute, out var uri)
            && _notarialDomains.Contains(uri.Host))
        {
            return ParseUrl(uri, qrText);
        }

        // Попытка 2: Структурированные данные key=value;key=value
        if (qrText.Contains('='))
        {
            return ParseKeyValue(qrText);
        }

        // Попытка 3: Простой реестровый номер
        if (IsRegistryNumber(qrText))
        {
            return new NotarizationQrData(
                RegistryNumber: qrText,
                NotarizationDate: null,
                NotaryFullName: null,
                NotaryDistrict: null,
                DocumentType: null,
                ApplicantName: null,
                RawUrl: qrText);
        }

        _logger.LogDebug("Не удалось распознать формат QR-данных: {Text}", qrText[..Math.Min(200, qrText.Length)]);
        return null;
    }

    private NotarizationQrData ParseUrl(Uri uri, string rawUrl)
    {
        var query = HttpUtility.ParseQueryString(uri.Query);

        var registryNumber = GetFirstValue(query, "id", "reg", "registry_number", "regnum", "номер");
        var dateStr = GetFirstValue(query, "date", "dt", "notarization_date", "дата");
        var notary = GetFirstValue(query, "notary", "notary_name", "notary_fio", "нотариус");
        var district = GetFirstValue(query, "district", "notary_district", "округ");
        var docType = GetFirstValue(query, "type", "doc_type", "document_type", "вид");
        var applicant = GetFirstValue(query, "applicant", "applicant_name", "заявитель");

        DateOnly? notarizationDate = ParseDate(dateStr);

        _logger.LogInformation(
            "QR URL распарсен: рег.номер={RegistryNumber}, дата={Date}, нотариус={Notary}",
            registryNumber, notarizationDate?.ToString("O"), notary);

        return new NotarizationQrData(
            RegistryNumber: registryNumber,
            NotarizationDate: notarizationDate,
            NotaryFullName: notary,
            NotaryDistrict: district,
            DocumentType: docType,
            ApplicantName: applicant,
            RawUrl: rawUrl);
    }

    private NotarizationQrData ParseKeyValue(string data)
    {
        var parts = data.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var part in parts)
        {
            var eqIndex = part.IndexOf('=');
            if (eqIndex > 0)
            {
                var key = part[..eqIndex].Trim();
                var value = part[(eqIndex + 1)..].Trim();
                dict[key] = value;
            }
        }

        dict.TryGetValue("id", out var registryNumber);
        if (registryNumber is null)
            dict.TryGetValue("reg", out registryNumber);
        registryNumber ??= dict.GetValueOrDefault("registry_number");
        registryNumber ??= dict.GetValueOrDefault("номер");

        dict.TryGetValue("date", out var dateStr);
        dateStr ??= dict.GetValueOrDefault("дата");

        dict.TryGetValue("notary", out var notary);
        notary ??= dict.GetValueOrDefault("нотариус");

        dict.TryGetValue("district", out var district);
        district ??= dict.GetValueOrDefault("округ");

        dict.TryGetValue("type", out var docType);
        docType ??= dict.GetValueOrDefault("вид");

        dict.TryGetValue("applicant", out var applicant);
        applicant ??= dict.GetValueOrDefault("заявитель");

        return new NotarizationQrData(
            RegistryNumber: registryNumber,
            NotarizationDate: ParseDate(dateStr),
            NotaryFullName: notary,
            NotaryDistrict: district,
            DocumentType: docType,
            ApplicantName: applicant,
            RawUrl: data);
    }

    private static string? GetFirstValue(System.Collections.Specialized.NameValueCollection query, params string[] keys)
    {
        foreach (var key in keys)
        {
            var value = query[key];
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }
        return null;
    }

    private static DateOnly? ParseDate(string? dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr))
            return null;

        var formats = new[]
        {
            "yyyy-MM-dd",
            "dd.MM.yyyy",
            "dd/MM/yyyy",
            "yyyy-MM-ddTHH:mm:ss",
            "O"
        };

        if (DateOnly.TryParseExact(dateStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return date;

        if (DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            return DateOnly.FromDateTime(dt);

        return null;
    }

    private static bool IsRegistryNumber(string value)
    {
        return value.Length >= 8 && value.All(c => char.IsDigit(c) || c == '-' || c == '/');
    }
}
