using Microsoft.EntityFrameworkCore;
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

        qrGroup.MapPost("/save-qr-data", SaveQrData)
            .Produces(200)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);
    }

    private static async Task<IResult> SaveQrData(
        SaveQrDataRequest request,
        IApplicationDbContext db,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        var logger = loggerFactory.CreateLogger("NotarizationQr.SaveQrData");

        try
        {
            if (request.FileId == Guid.Empty)
                return Results.BadRequest(new { error = "FileId обязателен." });

            var file = await db.Files.FirstOrDefaultAsync(f => f.Id == request.FileId, ct);
            if (file is null)
                return Results.NotFound(new { error = $"Файл {request.FileId} не найден." });

            file.QrRawUrl = request.RawUrl;
            file.QrRegistryNumber = request.RegistryNumber;
            file.QrNotaryFullName = request.NotaryFullName;
            file.QrDocumentType = request.DocumentType;
            file.QrApplicantName = request.ApplicantName;

            if (DateOnly.TryParse(request.NotarizationDate, out var parsedDate))
                file.QrNotarizationDate = parsedDate;

            await db.SaveChangesAsync(ct);

            logger.LogInformation(
                "QR-данные сохранены для файла {FileId}: рег.номер={RegistryNumber}, нотариус={Notary}",
                request.FileId, request.RegistryNumber, request.NotaryFullName);

            return Results.Ok(new { message = "QR-данные сохранены." });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Ошибка сохранения QR-данных для файла {FileId}", request.FileId);
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ReadQrCode(
        HttpRequest request,
        IQrCodeReaderService qrReader,
        INotarizationQrParser qrParser,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        var logger = loggerFactory.CreateLogger("NotarizationQr.ReadQrCode");

        try
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
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Ошибка чтения QR-кода");
            return Results.BadRequest(new { error = $"Ошибка чтения QR-кода: {ex.Message}" });
        }
    }

    /// <summary>DTO для сохранения QR-данных файла.</summary>
    public record SaveQrDataRequest(
        Guid FileId,
        string? RegistryNumber,
        string? NotarizationDate,
        string? NotaryFullName,
        string? DocumentType,
        string? ApplicantName,
        string? RawUrl);

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
