using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Infrastructure.Services;

namespace SamorodinkaTech.Fiducia.BoardPortal;

/// <summary>
/// Minimal API endpoints для чтения QR-кодов с нотариальных документов.
/// </summary>
public static class NotarizationQrEndpoints
{
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

            // Проверяем, есть ли уже запись для этого файла
            var existing = await db.FileNotarizations.FirstOrDefaultAsync(fn => fn.FileId == request.FileId, ct);

            DateOnly? parsedDate = null;
            if (DateOnly.TryParse(request.NotarizationDate, out var d))
                parsedDate = d;

            if (existing is not null)
            {
                existing.RawUrl = request.RawUrl;
                existing.RegistryNumber = request.RegistryNumber;
                existing.NotaryFullName = request.NotaryFullName;
                existing.DocumentType = request.DocumentType;
                existing.ApplicantName = request.ApplicantName;
                if (parsedDate.HasValue)
                    existing.NotarizationDate = parsedDate;
            }
            else
            {
                db.FileNotarizations.Add(new Domain.Entities.FileNotarization
                {
                    Id = Guid.NewGuid(),
                    FileId = request.FileId,
                    RawUrl = request.RawUrl,
                    RegistryNumber = request.RegistryNumber,
                    NotaryFullName = request.NotaryFullName,
                    DocumentType = request.DocumentType,
                    ApplicantName = request.ApplicantName,
                    NotarizationDate = parsedDate
                });
            }

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
        IOptions<QrCodeReaderOptions> options,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        var logger = loggerFactory.CreateLogger("NotarizationQr.ReadQrCode");
        var opts = options.Value;
        var allowedImageTypes = new HashSet<string>(opts.AllowedImageContentTypes, StringComparer.OrdinalIgnoreCase);
        var allowedExtensions = new HashSet<string>(opts.AllowedExtensions, StringComparer.OrdinalIgnoreCase);

        try
        {
            if (!request.HasFormContentType)
                return Results.BadRequest(new { error = "Ожидается multipart/form-data." });

            var form = await request.ReadFormAsync(ct);
            var file = form.Files.GetFile("file");

            if (file is null || file.Length == 0)
                return Results.BadRequest(new { error = "Файл не загружен." });

            if (file.Length > opts.MaxFileSizeBytes)
                return Results.BadRequest(new { error = $"Файл слишком большой. Максимум: {opts.MaxFileSizeBytes / 1024 / 1024} МБ." });

            var contentType = file.ContentType ?? "";
            var isImage = allowedImageTypes.Contains(contentType);
            var isPdf = string.Equals(contentType, opts.PdfContentType, StringComparison.OrdinalIgnoreCase);

            if (!isImage && !isPdf)
            {
                var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
                if (extension is not null)
                {
                    isImage = allowedExtensions.Contains(extension) && !string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase);
                    isPdf = string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase);
                }
            }

            if (!isImage && !isPdf)
            {
                var extList = string.Join(", ", opts.AllowedExtensions);
                return Results.BadRequest(new { error = $"Поддерживаются только файлы с расширениями: {extList}." });
            }

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
