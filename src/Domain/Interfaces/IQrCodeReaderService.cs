namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Результат парсинга QR-кода нотариального документа.
/// </summary>
public record NotarizationQrData(
    string? RegistryNumber,
    DateOnly? NotarizationDate,
    string? NotaryFullName,
    string? NotaryDistrict,
    string? DocumentType,
    string? ApplicantName,
    string RawUrl);

/// <summary>
/// Сервис чтения QR-кодов из файлов (изображения, PDF).
/// </summary>
public interface IQrCodeReaderService
{
    /// <summary>Считывает QR-код из изображения (PNG/JPEG). Возвращает decoded text или null.</summary>
    Task<string?> ReadFromImageAsync(Stream imageStream, CancellationToken ct = default);

    /// <summary>Считывает QR-код из PDF-файла (извлекает изображения со страниц).</summary>
    Task<string?> ReadFromPdfAsync(Stream pdfStream, CancellationToken ct = default);
}

/// <summary>
/// Сервис парсинга декодированного текста QR-кода в структурированные данные нотариального документа.
/// </summary>
public interface INotarizationQrParser
{
    /// <summary>Парсит декодированный текст QR-кода в структурированные данные.</summary>
    NotarizationQrData? Parse(string qrText);
}
