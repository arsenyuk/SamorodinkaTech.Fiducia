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
/// Реализация ICeoResignationService: уведомление ГД об увольнении (ст. 280 ТК РФ).
/// Логика извлечена из CeoResignationEndpoints (POST /send).
/// </summary>
public class CeoResignationService : ICeoResignationService
{
    private readonly IDbContextFactory<FiduciaDbContext> _dbFactory;
    private readonly INotificationService _notificationService;
    private readonly NotificationTextBuilder _textBuilder;
    private readonly ICeoResignationDocxGenerator _docxGenerator;
    private readonly IFileStorage _fileStorage;
    private readonly ILogger<CeoResignationService> _logger;

    /// <summary>
    /// Минимальное количество дней до даты увольнения (ст. 280 ТК РФ — 1 месяц).
    /// </summary>
    private const int MinDaysBeforeResignation = 30;

    /// <summary>
    /// MIME-тип DOCX-файлов.
    /// </summary>
    private const string DocxContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    /// <summary>
    /// Тип уведомления.
    /// </summary>
    private const string NotificationTypeCode = "CEO_RESIGNATION";

    /// <summary>
    /// Тип файла для сохранения.
    /// </summary>
    private const string FileType = "CEO_RESIGNATION";

    /// <summary>
    /// Префикс имени DOCX-файла.
    /// </summary>
    private const string FilePrefix = "Уведомление_ГД_увольнение_";

    /// <summary>
    /// Шаблон имени файла: {prefix}{sanitized_name}.docx.
    /// </summary>
    private static readonly Regex SanitizePattern = new(@"[^\w\-]", RegexOptions.Compiled);

    /// <summary>
    /// Дефолтная повестка ВОСУ при увольнении ГД.
    /// </summary>
    private static readonly string[] DefaultAgendaItems =
    {
        "Досрочное прекращение полномочий Генерального директора",
        "Избрание нового Генерального директора"
    };

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="CeoResignationService"/>.
    /// </summary>
    public CeoResignationService(
        IDbContextFactory<FiduciaDbContext> dbFactory,
        INotificationService notificationService,
        NotificationTextBuilder textBuilder,
        ICeoResignationDocxGenerator docxGenerator,
        IFileStorage fileStorage,
        ILogger<CeoResignationService> logger)
    {
        _dbFactory = dbFactory;
        _notificationService = notificationService;
        _textBuilder = textBuilder;
        _docxGenerator = docxGenerator;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<CeoResignationResult> SendAsync(CeoResignationSendModel model, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(ct);

        // Получаем текущего пользователя
        var currentUserId = model.UserId;

        // Проверяем что пользователь — ГД
        var ceoParticipant = await ctx.BoardParticipants
            .Where(x => x.LegalEntityId == model.LegalEntityId && x.IsGeneralDirector && x.IsActive)
            .Include(x => x.EcosystemParticipant)
            .FirstOrDefaultAsync(ct);

        if (ceoParticipant is null)
            throw new InvalidOperationException("Текущий пользователь не является Генеральным директором");

        // Получаем данные ГД
        var ceoUser = ceoParticipant.EcosystemParticipant?.UserId.HasValue == true
            ? await ctx.Users.FindAsync(new object[] { ceoParticipant.EcosystemParticipant.UserId.Value }, ct)
            : null;
        var ceoFullName = ceoUser is not null
            ? string.Join(" ", new[] { ceoUser.LastName, ceoUser.FirstName, ceoUser.MiddleName }
                .Where(x => !string.IsNullOrWhiteSpace(x)))
            : ceoParticipant.Person?.FullName
              ?? throw new InvalidOperationException("У ГД не заполнены данные ФЛ (Person)");

        // Валидация: до даты увольнения не менее 30 дней (ст. 280 ТК РФ)
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var daysUntilResignation = model.ResignationDate.DayNumber - today.DayNumber;
        if (daysUntilResignation < MinDaysBeforeResignation)
        {
            throw new InvalidOperationException(
                $"До даты увольнения должно оставаться не менее {MinDaysBeforeResignation} календарных дней " +
                $"(ст. 280 ТК РФ). Указана дата {model.ResignationDate:dd.MM.yyyy} — осталось {daysUntilResignation} дн.");
        }

        var legalEntity = await ctx.LegalEntities.FindAsync(new object[] { model.LegalEntityId }, ct);
        if (legalEntity is null)
            throw new InvalidOperationException("Юридическое лицо не найдено");

        // Загружаем активных участников
        var participants = await ctx.BoardParticipants
            .Where(x => x.LegalEntityId == model.LegalEntityId && x.IsActive)
            .Include(x => x.EcosystemParticipant)
            .Include(x => x.Person)
            .ToListAsync(ct);

        if (participants.Count == 0)
            throw new InvalidOperationException("Нет активных участников для отправки уведомления");

        int sentCount = 0;
        var fileIds = new List<Guid>();

        foreach (var participant in participants)
        {
            var participantName = participant.ParticipantType == "FL"
                ? participant.Person?.FullName
                : participant.CompanyName;

            // Формируем текст уведомления
            var (title, body) = await _textBuilder.BuildCeoResignationAsync(
                legalEntity.Name, participantName, ceoFullName, model.ResignationDate);

            // Отправляем уведомление
            var recipientUserId = participant.EcosystemParticipant?.UserId;
            if (recipientUserId.HasValue)
            {
                await _notificationService.SendAsync(
                    NotificationTypeCode,
                    title, body,
                    recipientUserId.Value,
                    cancellationToken: ct);
                sentCount++;
            }

            // Определяем адрес участника
            string? participantAddress = participant.ParticipantType == "FL"
                ? (participant.PersonId.HasValue
                    ? ctx.IdentityDocuments.FirstOrDefault(x => x.PersonId == participant.PersonId.Value && x.IsActive)?.RegistrationAddress
                    : null)
                : participant.CompanyAddress;

            // Генерируем DOCX
            var agendaItems = model.AgendaItems ?? DefaultAgendaItems.ToList();
            var docxData = new CeoResignationData
            {
                LegalEntityName = legalEntity.Name,
                LegalEntityOgrn = legalEntity.Ogrn,
                LegalEntityInn = legalEntity.Inn,
                ParticipantFullName = participantName ?? "Неизвестный",
                ParticipantAddress = participantAddress,
                CeoName = ceoFullName,
                ResignationDate = model.ResignationDate,
                NotificationDate = today,
                ReviewLocation = model.ReviewLocation,
                AgendaItems = agendaItems
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
            .FirstOrDefaultAsync(ct);

        if (requestTypeId != Guid.Empty)
        {
            var vosuAgenda = model.AgendaItems ?? DefaultAgendaItems.ToList();

            var shareRequest = new ShareRequest
            {
                Id = Guid.NewGuid(),
                LegalEntityId = model.LegalEntityId,
                ParticipantId = ceoParticipant.Id,
                RequestTypeId = requestTypeId,
                Status = "submitted",
                Payload = System.Text.Json.JsonSerializer.Serialize(new
                {
                    type = "CEO_RESIGNATION",
                    resignationDate = model.ResignationDate.ToString("yyyy-MM-dd"),
                    agenda = vosuAgenda
                }),
                SubmittedToCeoAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId,
                VisibleToAll = false
            };
            ctx.ShareRequests.Add(shareRequest);
        }

        await ctx.SaveChangesAsync(ct);

        _logger.LogInformation("Уведомление ГД об увольнении: отправлено {Count}, файлов {FileCount}, " +
            "дата увольнения {Date}", sentCount, fileIds.Count, model.ResignationDate);

        return new CeoResignationResult
        {
            SentCount = sentCount,
            FileCount = fileIds.Count,
            ResignationDate = model.ResignationDate,
            DaysUntilResignation = daysUntilResignation
        };
    }
}
