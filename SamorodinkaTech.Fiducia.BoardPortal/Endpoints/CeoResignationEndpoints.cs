using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Helpers;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models;
using SamorodinkaTech.Fiducia.Domain.Services;
using SamorodinkaTech.Fiducia.Infrastructure.Common.Exceptions;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.BoardPortal;

/// <summary>
/// Minimal API endpoints для уведомления ГД об увольнении (ст. 280 ТК РФ).
/// </summary>
public static class CeoResignationEndpoints
{
    /// <summary>Минимальное количество дней до даты увольнения (ст. 280 ТК РФ — 1 месяц).</summary>
    private const int MinDaysBeforeResignation = 30;

    public static void MapCeoResignationEndpoints(this WebApplication app)
    {
        var ceoResignation = app.MapGroup("/api/ceo-resignation")
            .RequireAuthorization()
            .WithTags("CEO Resignation");

        // POST: отправить уведомление + создать требование ВОСУ
        ceoResignation.MapPost("/send", async (
            CeoResignationRequest request,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            INotificationService notificationService,
            NotificationTextBuilder textBuilder,
            ICeoResignationDocxGenerator docxGenerator,
            IFileStorage fileStorage,
            ILoggerFactory loggerFactory,
            HttpContext http) =>
        {
            var logger = loggerFactory.CreateLogger("CeoResignation.Send");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();

                var leId = await LegalEntityHelper.GetLegalEntityIdAsync(ctx, http);
                if (leId is null)
                    return Results.BadRequest(new { error = "Юридическое лицо не выбрано" });

                // Получаем текущего пользователя
                var userIdStr = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? http.User.FindFirst("sub")?.Value;
                Guid.TryParse(userIdStr, out var currentUserId);

                // Проверяем что пользователь — ГД
                var ceoParticipant = await ctx.BoardParticipants
                    .Where(x => x.LegalEntityId == leId.Value && x.IsGeneralDirector && x.IsActive)
                    .Include(x => x.EcosystemParticipant)
                    .FirstOrDefaultAsync();

                if (ceoParticipant is null)
                    return Results.BadRequest(new { error = "Текущий пользователь не является Генеральным директором" });

                // Получаем данные ГД
                var ceoUser = ceoParticipant.EcosystemParticipant?.UserId.HasValue == true
                    ? await ctx.Users.FindAsync(ceoParticipant.EcosystemParticipant.UserId.Value)
                    : null;
                var ceoFullName = ceoUser is not null
                    ? string.Join(" ", new[] { ceoUser.LastName, ceoUser.FirstName, ceoUser.MiddleName }
                        .Where(x => !string.IsNullOrWhiteSpace(x)))
                    : ceoParticipant.FullName ?? "Генеральный директор";

                // Валидация: до даты увольнения не менее 30 дней (ст. 280 ТК РФ)
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var daysUntilResignation = request.ResignationDate.DayNumber - today.DayNumber;
                if (daysUntilResignation < MinDaysBeforeResignation)
                {
                    logger.LogWarning("Попытка отправить уведомление об увольнении с датой {Date} — " +
                        "до даты увольнения {Days} дней (минимум {MinDays})",
                        request.ResignationDate, daysUntilResignation, MinDaysBeforeResignation);
                    return Results.BadRequest(new
                    {
                        error = $"До даты увольнения должно оставаться не менее {MinDaysBeforeResignation} календарных дней " +
                                $"(ст. 280 ТК РФ). Указана дата {request.ResignationDate:dd.MM.yyyy} — осталось {daysUntilResignation} дн."
                    });
                }

                var legalEntity = await ctx.LegalEntities.FindAsync(leId.Value);
                if (legalEntity is null)
                    return Results.BadRequest(new { error = "Юридическое лицо не найдено" });

                // Загружаем активных участников
                var participants = await ctx.BoardParticipants
                    .Where(x => x.LegalEntityId == leId.Value && x.IsActive)
                    .Include(x => x.EcosystemParticipant)
                    .ToListAsync();

                if (participants.Count == 0)
                    return Results.BadRequest(new { error = "Нет активных участников для отправки уведомления" });

                int sentCount = 0;
                var fileIds = new List<Guid>();

                foreach (var participant in participants)
                {
                    var participantName = participant.ParticipantType == "FL"
                        ? participant.FullName ?? "Участник"
                        : participant.CompanyName ?? "Участник";

                    // Формируем текст уведомления
                    var (title, body) = await textBuilder.BuildCeoResignationAsync(
                        legalEntity.Name, participantName, ceoFullName, request.ResignationDate);

                    // Отправляем уведомление
                    var recipientUserId = participant.EcosystemParticipant?.UserId;
                    if (recipientUserId.HasValue)
                    {
                        await notificationService.SendAsync(
                            "CEO_RESIGNATION",
                            title, body,
                            recipientUserId.Value);
                        sentCount++;
                    }

                    // Генерируем DOCX
                    var docxData = new CeoResignationData
                    {
                        LegalEntityName = legalEntity.Name,
                        LegalEntityOgrn = legalEntity.Ogrn,
                        LegalEntityInn = legalEntity.Inn,
                        ParticipantFullName = participantName,
                        ParticipantAddress = participant.ParticipantType == "FL"
                            ? participant.PassportRegistrationAddress
                            : participant.CompanyAddress,
                        CeoName = ceoFullName,
                        ResignationDate = request.ResignationDate,
                        NotificationDate = today,
                        ReviewLocation = request.ReviewLocation,
                        AgendaItems = request.AgendaItems ?? new List<string>
                        {
                            "Досрочное прекращение полномочий Генерального директора",
                            "Избрание нового Генерального директора"
                        }
                    };

                    var docxBytes = await docxGenerator.GenerateAsync(docxData);
                    using var ms = new MemoryStream(docxBytes);
                    var sanitized = Regex.Replace(participantName, @"[^\w\-]", "_");
                    var fileName = $"Уведомление_ГД_увольнение_{sanitized}.docx";

                    var storageKey = await fileStorage.SaveAsync(ms, fileName,
                        "application/vnd.openxmlformats-officedocument.wordprocessingml.document");

                    var fileEntry = new FileEntry
                    {
                        Id = Guid.NewGuid(),
                        OriginalName = fileName,
                        ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                        SizeBytes = docxBytes.Length,
                        StorageProvider = "LOCAL",
                        StorageKeyOrPath = storageKey,
                        Extension = "docx",
                        FileType = "CEO_RESIGNATION",
                        DisplayName = $"Уведомление об увольнении ГД — {participantName}",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = currentUserId
                    };
                    ctx.Files.Add(fileEntry);
                    fileIds.Add(fileEntry.Id);
                }

                // Создаём ShareRequest типа DEMAND_VOSU
                var requestTypeId = await ctx.RequestTypes
                    .Where(x => x.Code == "DEMAND_VOSU")
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync();

                if (requestTypeId != Guid.Empty)
                {
                    var vosuAgenda = request.AgendaItems ?? new List<string>
                    {
                        "Досрочное прекращение полномочий Генерального директора",
                        "Избрание нового Генерального директора"
                    };

                    var shareRequest = new ShareRequest
                    {
                        Id = Guid.NewGuid(),
                        LegalEntityId = leId.Value,
                        ParticipantId = ceoParticipant.Id,
                        RequestTypeId = requestTypeId,
                        Status = "submitted",
                        Payload = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            type = "CEO_RESIGNATION",
                            resignationDate = request.ResignationDate.ToString("yyyy-MM-dd"),
                            agenda = vosuAgenda
                        }),
                        SubmittedToCeoAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = currentUserId,
                        VisibleToAll = false
                    };
                    ctx.ShareRequests.Add(shareRequest);
                }

                await ctx.SaveChangesAsync();

                logger.LogInformation("Уведомление ГД об увольнении: отправлено {Count}, файлов {FileCount}, " +
                    "дата увольнения {Date}", sentCount, fileIds.Count, request.ResignationDate);

                return Results.Ok(new
                {
                    sentCount,
                    fileCount = fileIds.Count,
                    resignationDate = request.ResignationDate,
                    daysUntilResignation
                });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка отправки уведомления ГД об увольнении: {Error}",
                    ExceptionFlattener.Unwrap(ex));
                return Results.BadRequest(new { error = ExceptionFlattener.Unwrap(ex) });
            }
        });

        // GET: данные для предпросмотра
        ceoResignation.MapGet("/preview", async (
            IDbContextFactory<FiduciaDbContext> dbFactory,
            HttpContext http) =>
        {
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var leId = await LegalEntityHelper.GetLegalEntityIdAsync(ctx, http);
                if (leId is null)
                    return Results.BadRequest(new { error = "Юридическое лицо не выбрано" });

                var ceoParticipant = await ctx.BoardParticipants
                    .Where(x => x.LegalEntityId == leId.Value && x.IsGeneralDirector && x.IsActive)
                    .Include(x => x.EcosystemParticipant)
                    .FirstOrDefaultAsync();

                if (ceoParticipant is null)
                    return Results.BadRequest(new { error = "Текущий пользователь не является Генеральным директором" });

                var ceoUser = ceoParticipant.EcosystemParticipant?.UserId.HasValue == true
                    ? await ctx.Users.FindAsync(ceoParticipant.EcosystemParticipant.UserId.Value)
                    : null;
                var ceoName = ceoUser is not null
                    ? string.Join(" ", new[] { ceoUser.LastName, ceoUser.FirstName, ceoUser.MiddleName }
                        .Where(x => !string.IsNullOrWhiteSpace(x)))
                    : ceoParticipant.FullName ?? "Генеральный директор";

                var participantCount = await ctx.BoardParticipants
                    .CountAsync(x => x.LegalEntityId == leId.Value && x.IsActive);

                var legalEntity = await ctx.LegalEntities.FindAsync(leId.Value);

                return Results.Ok(new
                {
                    ceoName,
                    legalEntityName = legalEntity?.Name,
                    legalEntityOgrn = legalEntity?.Ogrn,
                    legalEntityInn = legalEntity?.Inn,
                    participantCount,
                    minResignationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(MinDaysBeforeResignation))
                });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ExceptionFlattener.Unwrap(ex) });
            }
        });
    }
}

/// <summary>
/// Запрос на отправку уведомления ГД об увольнении.
/// </summary>
public class CeoResignationRequest
{
    /// <summary>Плановая дата увольнения (не ранее чем через 30 дней).</summary>
    public DateOnly ResignationDate { get; init; }

    /// <summary>Место ознакомления с документами.</summary>
    public string? ReviewLocation { get; init; }

    /// <summary>Повестка ВОСУ (по умолчанию: избрание нового ГД).</summary>
    public List<string>? AgendaItems { get; init; }
}
