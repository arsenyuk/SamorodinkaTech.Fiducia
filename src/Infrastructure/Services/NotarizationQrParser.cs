using System.Globalization;
using System.Text;
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
        "notarialact.ru",
        "checkmark.eisnot.ru"
    };

    static NotarizationQrParser()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

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

        // checkmark.eisnot.ru: параметр "d" содержит base64-encoded данные
        var dParam = query["d"];
        if (!string.IsNullOrWhiteSpace(dParam))
            return ParseBase64Data(dParam, rawUrl);

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

    /// <summary>
    /// Парсит base64-encoded данные из параметра "d" (checkmark.eisnot.ru).
    /// Формат: строка1\nстрока2\n... где строки разделены переносами.
    /// </summary>
    private NotarizationQrData ParseBase64Data(string dParam, string rawUrl)
    {
        try
        {
            // Убираем URL-safe символы и декодируем
            var base64 = dParam.Replace('-', '+').Replace('_', '/');
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }

            var bytes = Convert.FromBase64String(base64);
            var text = Encoding.GetEncoding(1251).GetString(bytes);
            var lines = text.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length == 0)
                return new NotarizationQrData(null, null, null, null, null, null, rawUrl);

            // Строка 0: реестровый номер
            var registryNumber = lines.Length > 0 ? lines[0] : null;

            // Строка 1: дата;номер_нотариального_акта
            DateOnly? notarizationDate = null;
            if (lines.Length > 1)
            {
                var parts = lines[1].Split(';', StringSplitOptions.TrimEntries);
                if (parts.Length > 0)
                    notarizationDate = ParseDate(parts[0]);
            }

            // Строка 2: вид документа
            var docType = lines.Length > 2 ? lines[2] : null;

            // Строка 3: ФИО нотариуса;нотариальный округ
            string? notary = null;
            string? district = null;
            if (lines.Length > 3)
            {
                var parts = lines[3].Split(';', StringSplitOptions.TrimEntries);
                if (parts.Length > 0) notary = parts[0];
                if (parts.Length > 1) district = parts[1];
            }

            // Строка 4: заявитель
            var applicant = lines.Length > 4 ? lines[4] : null;

            _logger.LogInformation(
                "QR base64 распарсен: рег.номер={RegistryNumber}, дата={Date}, нотариус={Notary}",
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
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ошибка декодирования base64 параметра d");
            return new NotarizationQrData(null, null, null, null, null, null, rawUrl);
        }
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
