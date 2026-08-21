using SamorodinkaTech.Fiducia.Domain.Interfaces;

namespace SamorodinkaTech.Fiducia.BoardPortal;

/// <summary>
/// Minimal API endpoints для чтения QR-кодов с нотариальных документов.
/// </summary>
public static class NotarizationQrEndpoints
{
    private static readonly HashSet<string> AllowedImageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/png", "image/jpeg", "image/jpg", "image/bmp", "image/gif", "image/tiff"
    };

    private const string PdfContentType = "application/pdf";
    private const long MaxFileSize = 50 * 1024 * 1024; // 50 MB

    /// <summary>
    /// Регистрирует endpoint'ы для чтения QR-кодов.
    /// </summary>
    public static void MapNotarizationQrEndpoints(this WebApplication app)
    {
        var qrGroup = app.MapGroup("/api/notarization")
            .RequireAuthorization()
            .WithTags("Notarization QR");

        qrGroup.MapPost("/read-qr", ReadQrCode)
            .Produces<QrReadResult>()
            .ProducesProblem(400)
            .ProducesProblem(500);
    }

    private static async Task<IResult> ReadQrCode(
        HttpRequest request,
        IQrCodeReaderService qrReader,
        INotarizationQrParser qrParser,
        ILogger<Program> logger,
        CancellationToken ct)
    {
        if (!request.HasFormContentType)
            return Results.BadRequest(new { error = "Ожидается multipart/form-data." });

        var form = await request.ReadFormAsync(ct);
        var file = form.Files.GetFile("file");

        if (file is null || file.Length == 0)
            return Results.BadRequest(new { error = "Файл не загружен." });

        if (file.Length > MaxFileSize)
            return Results.BadRequest(new { error = $"Файл слишком большой. Максимум: {MaxFileSize / 1024 / 1024} МБ." });

        var contentType = file.ContentType ?? "";
        var isImage = AllowedImageTypes.Contains(contentType);
        var isPdf = string.Equals(contentType, PdfContentType, StringComparison.OrdinalIgnoreCase);

        if (!isImage && !isPdf)
        {
            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            isImage = extension is ".png" or ".jpg" or ".jpeg" or ".bmp" or ".gif" or ".tiff";
            isPdf = extension == ".pdf";
        }

        if (!isImage && !isPdf)
            return Results.BadRequest(new { error = "Поддерживаются только изображения (PNG, JPEG) и PDF-файлы." });

        await using var stream = file.OpenReadStream();

        string? qrText = isPdf
            ? await qrReader.ReadFromPdfAsync(stream, ct)
            : await qrReader.ReadFromImageAsync(stream, ct);

        if (qrText is null)
            return Results.BadRequest(new { error = "QR-код не найден на изображении. Убедитесь, что QR-код виден и не повреждён." });

        var qrData = qrParser.Parse(qrText);
        if (qrData is null)
            return Results.BadRequest(new { error = "QR-код считан, но данные не распознаны как нотариальный документ.", rawText = qrText });

        logger.LogInformation(
            "QR-код нотариального документа считан: рег.номер={RegistryNumber}, нотариус={Notary}, файл={FileName}",
            qrData.RegistryNumber, qrData.NotaryFullName, file.FileName);

        return Results.Ok(new QrReadResult(
            RegistryNumber: qrData.RegistryNumber,
            NotarizationDate: qrData.NotarizationDate?.ToString("yyyy-MM-dd"),
            NotaryFullName: qrData.NotaryFullName,
            NotaryDistrict: qrData.NotaryDistrict,
            DocumentType: qrData.DocumentType,
            ApplicantName: qrData.ApplicantName,
            RawUrl: qrData.RawUrl));
    }

    /// <summary>DTO ответа чтения QR-кода.</summary>
    public record QrReadResult(
        string? RegistryNumber,
        string? NotarizationDate,
        string? NotaryFullName,
        string? NotaryDistrict,
        string? DocumentType,
        string? ApplicantName,
        string RawUrl);
}
