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
/// Minimal API endpoints для отправки уведомлений ВОСУ и генерации DOCX (Board Portal).
/// </summary>
public static class VosuNotificationEndpoints
{
    public static void MapVosuNotificationEndpoints(this WebApplication app)
    {
        var vosuNotifications = app.MapGroup("/api/vosu-notifications")
            .RequireAuthorization()
            .WithTags("VOSU Notifications");

        // POST: отправить уведомления участникам + сформировать DOCX
        vosuNotifications.MapPost("/send", async (
            VosuNotificationRequest request,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            INotificationService notificationService,
            NotificationTextBuilder textBuilder,
            IVosuNotificationDocxGenerator docxGenerator,
            IFileStorage fileStorage,
            ILoggerFactory loggerFactory,
            HttpContext http) =>
        {
            var logger = loggerFactory.CreateLogger("VosuNotifications.Send");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();

                var leId = await LegalEntityHelper.GetLegalEntityIdAsync(ctx, http);
                if (leId is null)
                    return Results.BadRequest(new { error = "Юридическое лицо не выбрано" });

                var meeting = await ctx.OsaMeetings
                    .Include(x => x.LegalEntity)
                    .FirstOrDefaultAsync(x => x.Id == request.MeetingId && x.LegalEntityId == leId.Value);
                if (meeting is null)
                    return Results.BadRequest(new { error = "Собрание не найдено" });

                var legalEntity = meeting.LegalEntity;
                if (legalEntity is null)
                    return Results.BadRequest(new { error = "Юридическое лицо не найдено" });

                // Обновляем поля собрания
                meeting.GosaWindowStart = request.MeetingDate;
                meeting.MeetingStartTime = request.MeetingStartTime;
                meeting.MeetingVenue = request.MeetingVenue;
                meeting.RegistrationStartTime = request.RegistrationStartTime;
                await ctx.SaveChangesAsync();

                // Загружаем активных участников через EcosystemParticipant для получения UserId
                var participants = await ctx.BoardParticipants
                    .Where(x => x.LegalEntityId == leId.Value && x.IsActive)
                    .Include(x => x.EcosystemParticipant)
                    .ToListAsync();

                if (participants.Count == 0)
                    return Results.BadRequest(new { error = "Нет активных участников для отправки уведомления" });

                // Получаем userId текущего пользователя (ГД)
                var userIdStr = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? http.User.FindFirst("sub")?.Value;
                Guid.TryParse(userIdStr, out var currentUserId);

                var currentUser = await ctx.Users.FindAsync(currentUserId);
                var ceoFullName = currentUser is not null
                    ? string.Join(" ", new[] { currentUser.LastName, currentUser.FirstName, currentUser.MiddleName }
                        .Where(x => !string.IsNullOrWhiteSpace(x)))
                    : request.CeoName;

                int sentCount = 0;
                var fileIds = new List<Guid>();

                foreach (var participant in participants)
                {
                    var participantName = participant.ParticipantType == "FL"
                        ? participant.FullName ?? "Участник"
                        : participant.CompanyName ?? "Участник";

                    // Формируем текст уведомления
                    var (title, body) = await textBuilder.BuildVosuAgendaChangeAsync(
                        legalEntity.Name, participantName,
                        request.MeetingDate, request.MeetingStartTime, request.MeetingVenue);

                    // Отправляем уведомление в системе (через EcosystemParticipant.UserId)
                    var recipientUserId = participant.EcosystemParticipant?.UserId;
                    if (recipientUserId.HasValue)
                    {
                        await notificationService.SendAsync(
                            "VOSU_AGENDA_CHANGE",
                            title, body,
                            recipientUserId.Value,
                            meetingId: meeting.Id);
                        sentCount++;
                    }

                    // Генерируем DOCX для участника
                    var docxData = new VosuNotificationData
                    {
                        LegalEntityName = legalEntity.Name,
                        LegalEntityOgrn = legalEntity.Ogrn,
                        LegalEntityInn = legalEntity.Inn,
                        ParticipantFullName = participantName,
                        ParticipantAddress = participant.ParticipantType == "FL"
                            ? participant.PassportRegistrationAddress
                            : participant.CompanyAddress,
                        MeetingDate = request.MeetingDate,
                        MeetingStartTime = request.MeetingStartTime,
                        MeetingVenue = request.MeetingVenue,
                        RegistrationStartTime = request.RegistrationStartTime,
                        AgendaItems = request.AgendaItems ?? new List<string>(),
                        InitiatorName = request.InitiatorName,
                        InitiatorSharePercent = request.InitiatorSharePercent,
                        DemandReceivedDate = request.DemandReceivedDate,
                        ReviewLocation = request.ReviewLocation,
                        CeoName = ceoFullName,
                        NotificationDate = DateOnly.FromDateTime(DateTime.UtcNow),
                        IsAgendaChange = true
                    };

                    var docxBytes = await docxGenerator.GenerateAsync(docxData);
                    using var ms = new MemoryStream(docxBytes);
                    var sanitized = Regex.Replace(participantName, @"[^\w\-]", "_");
                    var fileName = $"Уведомление_ВОСУ_{sanitized}.docx";

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
                        FileType = "VOSU_NOTIFICATION",
                        DisplayName = $"Уведомление ВОСУ — {participantName}",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = currentUserId
                    };
                    ctx.Files.Add(fileEntry);

                    var link = new OsaMeetingFile
                    {
                        Id = Guid.NewGuid(),
                        OsaMeetingId = meeting.Id,
                        FileId = fileEntry.Id
                    };
                    ctx.OsaMeetingFiles.Add(link);
                    fileIds.Add(fileEntry.Id);
                }

                await ctx.SaveChangesAsync();

                logger.LogInformation("Отправлено уведомлений ВОСУ: {Count}, файлов DOCX: {FileCount}",
                    sentCount, fileIds.Count);

                return Results.Ok(new
                {
                    sentCount,
                    fileCount = fileIds.Count,
                    meetingId = meeting.Id
                });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка отправки уведомлений ВОСУ: {Error}",
                    ExceptionFlattener.Unwrap(ex));
                return Results.BadRequest(new { error = ExceptionFlattener.Unwrap(ex) });
            }
        });

        // GET: данные для предпросмотра формы уведомления
        vosuNotifications.MapGet("/preview/{meetingId:guid}", async (
            Guid meetingId,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            HttpContext http) =>
        {
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var leId = await LegalEntityHelper.GetLegalEntityIdAsync(ctx, http);
                if (leId is null)
                    return Results.BadRequest(new { error = "Юридическое лицо не выбрано" });

                var meeting = await ctx.OsaMeetings
                    .Include(x => x.LegalEntity)
                    .FirstOrDefaultAsync(x => x.Id == meetingId && x.LegalEntityId == leId.Value);
                if (meeting is null)
                    return Results.BadRequest(new { error = "Собрание не найдено" });

                var participantCount = await ctx.BoardParticipants
                    .CountAsync(x => x.LegalEntityId == leId.Value && x.IsActive);

                // Получаем текущего ГД
                var userIdStr = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? http.User.FindFirst("sub")?.Value;
                Guid.TryParse(userIdStr, out var currentUserId);
                var user = await ctx.Users.FindAsync(currentUserId);
                var ceoName = user is not null
                    ? string.Join(" ", new[] { user.LastName, user.FirstName, user.MiddleName }
                        .Where(x => !string.IsNullOrWhiteSpace(x)))
                    : "";

                return Results.Ok(new
                {
                    meetingId = meeting.Id,
                    title = meeting.Title,
                    meetingDate = meeting.GosaWindowStart,
                    meetingStartTime = meeting.MeetingStartTime,
                    meetingVenue = meeting.MeetingVenue,
                    registrationStartTime = meeting.RegistrationStartTime,
                    participantCount,
                    legalEntityName = meeting.LegalEntity?.Name,
                    legalEntityOgrn = meeting.LegalEntity?.Ogrn,
                    legalEntityInn = meeting.LegalEntity?.Inn,
                    ceoName
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
/// Запрос на отправку уведомлений ВОСУ.
/// </summary>
public class VosuNotificationRequest
{
    /// <summary>Идентификатор собрания.</summary>
    public Guid MeetingId { get; init; }

    /// <summary>Дата проведения собрания.</summary>
    public DateOnly MeetingDate { get; init; }

    /// <summary>Время начала собрания.</summary>
    public TimeOnly? MeetingStartTime { get; init; }

    /// <summary>Место проведения собрания.</summary>
    public string? MeetingVenue { get; init; }

    /// <summary>Время начала регистрации участников.</summary>
    public TimeOnly? RegistrationStartTime { get; init; }

    /// <summary>Вопросы повестки.</summary>
    public List<string>? AgendaItems { get; init; }

    /// <summary>ФИО инициатора (подавшего требование).</summary>
    public string? InitiatorName { get; init; }

    /// <summary>Доля инициатора (%).</summary>
    public decimal? InitiatorSharePercent { get; init; }

    /// <summary>Дата получения требования.</summary>
    public DateOnly? DemandReceivedDate { get; init; }

    /// <summary>Место ознакомления с документами.</summary>
    public string? ReviewLocation { get; init; }

    /// <summary>ФИО Генерального директора (подпись). Используется как fallback.</summary>
    public string CeoName { get; init; } = default!;
}
