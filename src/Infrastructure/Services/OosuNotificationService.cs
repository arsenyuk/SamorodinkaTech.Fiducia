using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models;
using SamorodinkaTech.Fiducia.Domain.Services;
using SamorodinkaTech.Fiducia.Infrastructure.Common.Exceptions;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Реализация IOosuNotificationService: отправка уведомлений ООСУ.
/// Логика извлечена из VosuNotificationEndpoints (POST /api/oosu-notifications/send).
/// </summary>
public class OosuNotificationService : IOosuNotificationService
{
    private readonly IDbContextFactory<FiduciaDbContext> _dbFactory;
    private readonly INotificationService _notificationService;
    private readonly NotificationTextBuilder _textBuilder;
    private readonly IOosuNotificationDocxGenerator _docxGenerator;
    private readonly IFileStorage _fileStorage;
    private readonly ILogger<OosuNotificationService> _logger;

    /// <summary>
    /// MIME-тип DOCX-файлов.
    /// </summary>
    private const string DocxContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    /// <summary>
    /// Тип уведомления ООСУ.
    /// </summary>
    private const string NotificationTypeCode = "OOSU_MEETING_NOTIFICATION";

    /// <summary>
    /// Тип файла для сохранения.
    /// </summary>
    private const string FileType = "OOSU_NOTIFICATION";

    /// <summary>
    /// Префикс имени DOCX-файла.
    /// </summary>
    private const string FilePrefix = "Уведомление_ООСУ_";

    /// <summary>
    /// Шаблон имени файла: {prefix}{sanitized_name}.docx.
    /// </summary>
    private static readonly Regex SanitizePattern = new(@"[^\w\-]", RegexOptions.Compiled);

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="OosuNotificationService"/>.
    /// </summary>
    public OosuNotificationService(
        IDbContextFactory<FiduciaDbContext> dbFactory,
        INotificationService notificationService,
        NotificationTextBuilder textBuilder,
        IOosuNotificationDocxGenerator docxGenerator,
        IFileStorage fileStorage,
        ILogger<OosuNotificationService> logger)
    {
        _dbFactory = dbFactory;
        _notificationService = notificationService;
        _textBuilder = textBuilder;
        _docxGenerator = docxGenerator;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<Guid>> SendAsync(OosuNotificationSendModel model, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(ct);

        var meeting = await ctx.OsaMeetings
            .Include(x => x.LegalEntity)
            .FirstOrDefaultAsync(x => x.Id == model.MeetingId && x.LegalEntityId == model.LegalEntityId, ct);
        if (meeting is null)
            throw new InvalidOperationException("Собрание не найдено");

        var legalEntity = meeting.LegalEntity;
        if (legalEntity is null)
            throw new InvalidOperationException("Юридическое лицо не найдено");

        meeting.GosaWindowStart = model.MeetingDate;
        meeting.MeetingStartTime = model.MeetingStartTime;
        meeting.MeetingVenue = model.MeetingVenue;
        meeting.RegistrationStartTime = model.RegistrationStartTime;
        await ctx.SaveChangesAsync(ct);

        var participants = await ctx.BoardParticipants
            .Where(x => x.LegalEntityId == model.LegalEntityId && x.IsActive)
            .Include(x => x.EcosystemParticipant)
            .ToListAsync(ct);

        if (participants.Count == 0)
            throw new InvalidOperationException("Нет активных участников для отправки уведомления");

        var currentUser = await ctx.Users.FindAsync(new object[] { model.UserId }, ct);
        var ceoFullName = currentUser is not null
            ? string.Join(" ", new[] { currentUser.LastName, currentUser.FirstName, currentUser.MiddleName }
                .Where(x => !string.IsNullOrWhiteSpace(x)))
            : model.CeoName;

        int sentCount = 0;
        var fileIds = new List<Guid>();

        foreach (var participant in participants)
        {
            var participantName = participant.ParticipantType == "FL"
                ? participant.Person?.FullName
                : participant.CompanyName;

            var (title, body) = await _textBuilder.BuildOosuMeetingNotificationAsync(
                legalEntity.Name, participantName,
                model.MeetingDate, model.MeetingStartTime, model.MeetingVenue);

            var recipientUserId = participant.EcosystemParticipant?.UserId;
            if (recipientUserId.HasValue)
            {
                await _notificationService.SendAsync(
                    NotificationTypeCode,
                    title, body,
                    recipientUserId.Value,
                    meetingId: meeting.Id,
                    cancellationToken: ct);
                sentCount++;
            }

            string? participantAddress = participant.ParticipantType == "FL"
                ? (participant.PersonId.HasValue
                    ? ctx.IdentityDocuments.FirstOrDefault(x => x.PersonId == participant.PersonId.Value && x.IsActive)?.RegistrationAddress
                    : null)
                : participant.CompanyAddress;

            var docxData = new OosuNotificationData
            {
                LegalEntityName = legalEntity.Name,
                LegalEntityOgrn = legalEntity.Ogrn,
                LegalEntityInn = legalEntity.Inn,
                ParticipantFullName = participantName ?? "Неизвестный",
                ParticipantAddress = participantAddress,
                MeetingDate = model.MeetingDate,
                MeetingStartTime = model.MeetingStartTime,
                MeetingVenue = model.MeetingVenue,
                RegistrationStartTime = model.RegistrationStartTime,
                AgendaItems = model.AgendaItems ?? new List<string>(),
                ReviewLocation = model.ReviewLocation,
                CeoName = ceoFullName,
                NotificationDate = DateOnly.FromDateTime(DateTime.UtcNow),
                FinancialYear = model.FinancialYear
            };

            var docxBytes = await _docxGenerator.GenerateAsync(docxData, ct);
            using var ms = new MemoryStream(docxBytes);
            var sanitized = SanitizePattern.Replace(participantName ?? "unknown", "_");
            var fileName = $"{FilePrefix}{sanitized}.docx";

            var storageKey = await _fileStorage.SaveAsync(ms, fileName, DocxContentType, ct);

            var fileEntry = new FileEntry
            {
                Id = Guid.NewGuid(),
                OriginalName = fileName,
                ContentType = DocxContentType,
                SizeBytes = docxBytes.Length,
                StorageProvider = "LOCAL",
                StorageKeyOrPath = storageKey,
                Extension = "docx",
                FileType = FileType,
                DisplayName = $"Уведомление ООСУ — {participantName}",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = model.UserId
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

        await ctx.SaveChangesAsync(ct);

        _logger.LogInformation("Отправлено уведомлений ООСУ: {Count}, файлов DOCX: {FileCount}",
            sentCount, fileIds.Count);

        return fileIds;
    }
}
