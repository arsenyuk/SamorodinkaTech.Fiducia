namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Настройки чтения QR-кодов из файлов.
/// Расширения файлов, которые могут быть распознаны для извлечения QR-кода.
/// </summary>
public class QrCodeReaderOptions
{
    /// <summary>
    /// Расширения файлов (с точкой, в нижнем регистре), допустимые для чтения QR-кода.
    /// Пример: [".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tiff", ".pdf"]
    /// </summary>
    public string[] AllowedExtensions { get; init; } =
    [
        ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tiff", ".pdf"
    ];

    /// <summary>
    /// MIME-типы изображений, допустимые для чтения QR-кода.
    /// Пример: ["image/png", "image/jpeg", "image/bmp", "image/gif", "image/tiff"]
    /// </summary>
    public string[] AllowedImageContentTypes { get; init; } =
    [
        "image/png", "image/jpeg", "image/jpg", "image/bmp", "image/gif", "image/tiff"
    ];

    /// <summary>MIME-тип PDF-файлов.</summary>
    public string PdfContentType { get; init; } = "application/pdf";

    /// <summary>Максимальный размер файла в байтах (по умолчанию 50 МБ).</summary>
    public long MaxFileSizeBytes { get; init; } = 50 * 1024 * 1024;
}
