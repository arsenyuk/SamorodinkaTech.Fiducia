using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Helpers;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models;
using SamorodinkaTech.Fiducia.Domain.Validation;
using SamorodinkaTech.Fiducia.Infrastructure.Common;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Реализация <see cref="IShareRequestWriteService"/>.
/// Заменяет POST/PUT/DELETE loopback HTTP-вызовы из Blazor-страниц к /api/share-requests.
/// Бизнес-логика извлечена из ShareRequestEndpoints.cs и ShareRequestItemEndpoints.cs.
/// </summary>
public class ShareRequestWriteService : IShareRequestWriteService
{
    private readonly IDbContextFactory<FiduciaDbContext> _dbFactory;
    private readonly ILogger<ShareRequestWriteService> _logger;
    private readonly ITemplateInstantiationService _templateService;
    private readonly IDocumentProvisionService _documentProvisionService;
    private readonly IVosuNotificationDocxGenerator _vosuDocxGenerator;
    private readonly IFileStorage _fileStorage;

    /// <summary>Длительность окна для отзыва нотариальной оферты (часы).</summary>
    private const double RevokeWindowHours = 24;

    /// <summary>Префикс URL уведомления для CSV-требований.</summary>
    private const string DemandUrlPrefix = "/ceo-demands/";

    public ShareRequestWriteService(
        IDbContextFactory<FiduciaDbContext> dbFactory,
        ILogger<ShareRequestWriteService> logger,
        ITemplateInstantiationService templateService,
        IDocumentProvisionService documentProvisionService,
        IVosuNotificationDocxGenerator vosuDocxGenerator,
        IFileStorage fileStorage)
    {
        _dbFactory = dbFactory;
        _logger = logger;
        _templateService = templateService;
        _documentProvisionService = documentProvisionService;
        _vosuDocxGenerator = vosuDocxGenerator;
        _fileStorage = fileStorage;
    }

    /// <inheritdoc />
    public async Task<Guid> CreateAsync(Guid userId, Guid requestTypeId, string? text, string? payload, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var leId = await ResolveLegalEntityIdAsync(ctx, userId);
        if (leId is null)
            throw new InvalidOperationException("Юридическое лицо не выбрано");

        var requestType = await ctx.RequestTypes.FindAsync(requestTypeId);
        if (requestType is null)
            throw new InvalidOperationException($"Неизвестный тип запроса: {requestTypeId}");

        // Проверяем доступность типа для текущего ЮЛ
        var le = await ctx.LegalEntities
            .Include(x => x.RefOkopf)
            .FirstOrDefaultAsync(x => x.Id == leId, ct);
        var okopfCode = le?.RefOkopf?.Code;
        var isLlc = OkopfTypeMapper.IsLlc(okopfCode);
        var isNjsc = okopfCode == OkopfTypeMapper.NjscCode;
        var isPjsc = OkopfTypeMapper.IsPjsc(okopfCode);

        if ((isLlc && !requestType.IsForLlc) || (isNjsc && !requestType.IsForNjsc) || (isPjsc && !requestType.IsForPjsc))
            throw new InvalidOperationException($"Тип запроса «{requestType.Name}» не доступен для данного типа организации");

        // Находим участника: user → ecosystemParticipant → boardParticipant
        var user = await ctx.Users.FindAsync(userId);
        var ecoParticipant = user is not null ? await PersonHelper.FindParticipantByUserIdAsync(ctx, user.Id, ct) : null;
        if (ecoParticipant is null)
            throw new InvalidOperationException("Пользователь не привязан к участнику экосистемы");

        var participant = await ctx.BoardParticipants
            .FirstOrDefaultAsync(p => p.LegalEntityId == leId.Value && p.EcosystemParticipantId == ecoParticipant.Id && p.IsActive, ct);
        if (participant is null)
            throw new InvalidOperationException("Не найден участник для текущего пользователя");

        // Специфичная валидация по типам
        var activeShareForValidation = await ctx.BoardParticipantShares
            .FirstOrDefaultAsync(s => s.ParticipantId == participant.Id && s.IsActive, ct);
        var validationError = await ValidateRequestTypeAsync(ctx, requestType, leId.Value, participant.Id, payload, activeShareForValidation?.SharePercent, ct);
        if (validationError is not null)
            throw new InvalidOperationException(validationError.Message);

        var entity = new ShareRequest
        {
            Id = Guid.NewGuid(),
            LegalEntityId = leId.Value,
            ParticipantId = participant.Id,
            RequestTypeId = requestTypeId,
            Status = "draft",
            Text = text,
            Payload = payload,
            CreatedBy = userId
        };

        ctx.ShareRequests.Add(entity);
        await ctx.SaveChangesAsync(ct);

        // Если выход требует единогласного решения ОСУ — помечаем для рассмотрения ОСУ
        if (requestType.Code == "EXIT_APPLICATION")
        {
            var charter = await ctx.LegalEntityCharters.FindAsync(leId.Value);
            if (charter?.ExitRequiresUnanimousOsu == true)
            {
                entity.IsCollective = true;
                entity.CollectiveStatus = "OSU_REVIEW";
                entity.Status = "submitted";
                await ctx.SaveChangesAsync(ct);
            }
        }

        _logger.LogInformation("Создан запрос {RequestId} типа {RequestTypeCode} пользователем {UserId}", entity.Id, requestType.Code, userId);
        return entity.Id;
    }

    /// <inheritdoc />
    public async Task UpdatePayloadAsync(Guid id, string? text, string? payload, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var item = await ctx.ShareRequests.FindAsync(id);
        if (item is null)
            throw new InvalidOperationException("Запрос не найден");

        ValidateStatusForOperation(item, "update");

        item.Text = text;
        if (payload is not null)
            item.Payload = payload;
        await ctx.SaveChangesAsync(ct);

        _logger.LogInformation("Обновлён запрос {RequestId}", id);
    }

    /// <inheritdoc />
    public async Task SubmitAsync(Guid id, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var item = await ctx.ShareRequests.FindAsync(id);
        if (item is null)
            throw new InvalidOperationException("Запрос не найден");

        ValidateStatusForOperation(item, "submit");

        item.Status = "submitted";
        await ctx.SaveChangesAsync(ct);

        _logger.LogInformation("Запрос {RequestId} отправлен", id);
    }

    /// <inheritdoc />
    public async Task RevokeAsync(Guid id, bool notarized, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var item = await ctx.ShareRequests
            .Include(r => r.RequestType)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (item is null)
            throw new InvalidOperationException("Запрос не найден");

        if (item.RequestType?.Code != "NOTARIAL_OFFER")
            throw new InvalidOperationException("Отзыв доступен только для нотариальных оферт");

        ValidateStatusForOperation(item, "revoke");

        if (item.RevokedAt.HasValue)
            throw new InvalidOperationException("Запрос уже отозван");

        if ((DateTime.UtcNow - item.CreatedAt).TotalHours > RevokeWindowHours)
            throw new InvalidOperationException("Прошло более 24 часов с момента создания");

        item.Status = "revoked";
        item.RevokedAt = DateTime.UtcNow;
        item.RevokedByNotarized = notarized;

        await ctx.SaveChangesAsync(ct);

        _logger.LogInformation("Запрос {RequestId} отозван (notarized={Notarized})", id, notarized);
    }

    /// <inheritdoc />
    public async Task SupportAsync(Guid id, Guid participantId, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var request = await ctx.ShareRequests.FindAsync(id);
        if (request is null || !request.IsCollective)
            throw new InvalidOperationException("Коллективное требование не найдено");

        if (request.CollectiveStatus != "COLLECTING")
            throw new InvalidOperationException("Поддержка доступна только для требований в статусе «Сбор поддержек»");

        var participant = await ctx.BoardParticipants
            .Include(p => p.Shares.Where(s => s.IsActive).Take(1))
            .FirstOrDefaultAsync(p => p.Id == participantId && p.IsActive, ct);
        if (participant is null)
            throw new InvalidOperationException("Участник не найден");

        // Проверяем: не поддерживал ли уже
        var existingSupport = await ctx.ShareRequestSupports
            .FirstOrDefaultAsync(s => s.ShareRequestId == id && s.ParticipantId == participantId && s.WithdrawnAt == null, ct);
        if (existingSupport is not null)
            throw new InvalidOperationException("Вы уже поддержали это требование");

        // Добавляем поддержку
        var support = new ShareRequestSupport
        {
            Id = Guid.NewGuid(),
            ShareRequestId = id,
            ParticipantId = participantId,
            SharePercentAtSupport = participant.Shares.FirstOrDefault()?.SharePercent ?? 0m,
            SupportedAt = DateTime.UtcNow
        };

        ctx.ShareRequestSupports.Add(support);

        // Пересчитываем суммарную долю
        request.TotalSupportPercent += support.SharePercentAtSupport;
        request.SupporterCount += 1;

        // Проверяем порог
        if (request.ThresholdPercent.HasValue
            && request.TotalSupportPercent >= request.ThresholdPercent.Value
            && request.CollectiveStatus == "COLLECTING")
        {
            var reqType = await ctx.RequestTypes.FindAsync(request.RequestTypeId);
            var charterForBoard = await ctx.LegalEntityCharters.FindAsync(request.LegalEntityId);
            bool isForBoard = charterForBoard?.BoardDecidesConveningOsu == true
                && reqType?.ConsideredByOsu == true;

            request.CollectiveStatus = isForBoard ? "BOARD_REVIEW" : "THRESHOLD_REACHED";

            if (isForBoard)
            {
                // Создаём пункт повестки заседания СД
                var activeBoard = await ctx.BoardsOfDirectors
                    .Include(b => b.OsaMeeting)
                    .Where(b => b.OsaMeeting!.LegalEntityId == request.LegalEntityId && b.EndedAt == null)
                    .OrderByDescending(b => b.ElectionYear)
                    .FirstOrDefaultAsync(ct);
                if (activeBoard is not null)
                {
                    var agendaItem = new AgendaItem
                    {
                        Id = Guid.NewGuid(),
                        BoardOfDirectorsId = activeBoard.Id,
                        LegalEntityId = request.LegalEntityId,
                        ShareRequestId = request.Id,
                        Title = $"Требование: {reqType?.Name ?? "Требование участника"}",
                        TargetType = "BOARD_MEETING",
                        Reason = "Требование участника (ст. 35 14-ФЗ)",
                        Status = "PENDING"
                    };
                    ctx.AgendaItems.Add(agendaItem);
                }
            }
            else
            {
                await NotifyCeoAsync(ctx, request, ct);
            }
        }

        await ctx.SaveChangesAsync(ct);

        _logger.LogInformation("Участник {ParticipantId} поддержал требование {RequestId}. Доля: {Total}%, статус: {Status}",
            participantId, id, request.TotalSupportPercent, request.CollectiveStatus);
    }

    /// <inheritdoc />
    public async Task WithdrawAsync(Guid id, Guid participantId, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var request = await ctx.ShareRequests.FindAsync(id);
        if (request is null || !request.IsCollective)
            throw new InvalidOperationException("Коллективное требование не найдено");

        if (request.CollectiveStatus != "COLLECTING")
            throw new InvalidOperationException("Отзыв доступен только для требований в статусе «Сбор поддержек»");

        var support = await ctx.ShareRequestSupports
            .FirstOrDefaultAsync(s => s.ShareRequestId == id && s.ParticipantId == participantId && s.WithdrawnAt == null, ct);
        if (support is null)
            throw new InvalidOperationException("Вы не поддерживали это требование");

        // Отзываем поддержку
        support.WithdrawnAt = DateTime.UtcNow;

        // Пересчитываем суммарную долю
        request.TotalSupportPercent -= support.SharePercentAtSupport;
        request.SupporterCount -= 1;

        await ctx.SaveChangesAsync(ct);

        _logger.LogInformation("Участник {ParticipantId} отозвал поддержку требования {RequestId}. Доля: {Total}%, статус: {Status}",
            participantId, id, request.TotalSupportPercent, request.CollectiveStatus);
    }

    /// <inheritdoc />
    public async Task DecideAsync(Guid id, bool approved, string? reason, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var decision = approved ? "ACCEPTED" : "REJECTED";

        var request = await ctx.ShareRequests
            .Include(r => r.RequestType)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
        if (request is null)
            throw new InvalidOperationException("Требование не найдено");

        ValidateStatusForOperation(request, "submit_decision");

        if (request.IsCollective)
        {
            request.CollectiveStatus = decision;
        }
        else
        {
            request.Status = decision;
        }
        request.CeoComment = reason;

        request.CeoDecisionAt = DateTime.UtcNow;
        request.CompletedAt = DateTime.UtcNow;
        request.SubmittedToCeoAt ??= DateTime.UtcNow;

        await ctx.SaveChangesAsync(ct);

        _logger.LogInformation("Решение по требованию {RequestId}: {Decision}", id, decision);

        // Если требование принято и тип = DEMAND_VOSU — создаём план ВОСУ
        if (approved && request.RequestType?.Code == "DEMAND_VOSU")
        {
            var orgIntentId = await CreateVosuPlanAsync(ctx, request.LegalEntityId, ct);
            if (orgIntentId.HasValue)
            {
                request.OrgIntentId = orgIntentId.Value;
                await ctx.SaveChangesAsync(ct);

                // Фиксируем: данное требование — инициирующее для ВОСУ
                var demandLink = new VosuDemandLink
                {
                    Id = Guid.NewGuid(),
                    OrgIntentId = orgIntentId.Value,
                    ShareRequestId = request.Id,
                    IsInitiating = true,
                    CreatedBy = request.DecidedByUserId ?? request.CreatedBy,
                    CreatedAt = DateTime.UtcNow
                };
                ctx.VosuDemandLinks.Add(demandLink);
                await ctx.SaveChangesAsync(ct);
            }
        }

        // Если требование принято и тип = DEMAND_VOSA — создаём план ВОСА
        if (approved && request.RequestType?.Code == "DEMAND_VOSA")
        {
            var orgIntentId = await CreateVosaPlanAsync(ctx, request.LegalEntityId, ct);
            if (orgIntentId.HasValue)
            {
                request.OrgIntentId = orgIntentId.Value;
                await ctx.SaveChangesAsync(ct);
            }
        }

        // Если требование принято и тип = REQUEST_INFORMATION — автоматически подгружаем документы
        if (approved && request.RequestType?.Code == "REQUEST_INFORMATION")
        {
            await _documentProvisionService.AutoProvisionDocumentsAsync(request.Id);
        }

        // Если требование принято и тип = EXIT_APPLICATION — помечаем участника как выбывшего
        if (approved && request.RequestType?.Code == "EXIT_APPLICATION")
        {
            var exitingParticipant = await ctx.BoardParticipants
                .FirstOrDefaultAsync(p => p.Id == request.ParticipantId, ct);
            if (exitingParticipant is not null)
            {
                exitingParticipant.ExitDate = DateOnly.FromDateTime(DateTime.UtcNow);
                exitingParticipant.IsActive = false;
                await ctx.SaveChangesAsync(ct);
            }
        }

        // Уведомляем всех поддержавших
        await NotifySupportersAsync(ctx, request, ct);
        await ctx.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task<Guid> CreateCollectiveAsync(Guid userId, Guid requestTypeId, string? text, string? payload, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var leId = await ResolveLegalEntityIdAsync(ctx, userId);
        if (leId is null)
            throw new InvalidOperationException("Юридическое лицо не выбрано");

        // Находим текущего участника
        var user = await ctx.Users.FindAsync(userId);
        var ecoParticipant = user is not null ? await PersonHelper.FindParticipantByUserIdAsync(ctx, user.Id, ct) : null;
        if (ecoParticipant is null)
            throw new InvalidOperationException("Пользователь не привязан к участнику экосистемы");

        var participant = await ctx.BoardParticipants
            .Include(p => p.Shares.Where(s => s.IsActive).Take(1))
            .FirstOrDefaultAsync(p => p.LegalEntityId == leId && p.EcosystemParticipantId == ecoParticipant.Id && p.IsActive, ct);
        if (participant is null)
            throw new InvalidOperationException("Не найден участник для текущего пользователя");

        var activeShare = participant.Shares.FirstOrDefault();

        // Проверяем тип требования
        var requestType = await ctx.RequestTypes.FindAsync(requestTypeId);
        if (requestType is null)
            throw new InvalidOperationException($"Неизвестный тип требования: {requestTypeId}");

        var le = await ctx.LegalEntities
            .Include(x => x.RefOkopf)
            .FirstOrDefaultAsync(x => x.Id == leId, ct);
        var okopfCode = le?.RefOkopf?.Code;
        var isLlc = OkopfTypeMapper.IsLlc(okopfCode);
        var isNjsc = okopfCode == OkopfTypeMapper.NjscCode;
        var isPjsc = OkopfTypeMapper.IsPjsc(okopfCode);

        if ((isLlc && !requestType.IsForLlc) || (isNjsc && !requestType.IsForNjsc) || (isPjsc && !requestType.IsForPjsc))
            throw new InvalidOperationException($"Тип требования «{requestType.Name}» не доступен для данного типа организации");

        // Определяем порог по типу запроса (ст. 35 14-ФЗ / ст. 55 208-ФЗ)
        var charter = await ctx.LegalEntityCharters.FindAsync(leId);
        decimal? charterThreshold = requestType.Code is "DEMAND_VOSU" or "DEMAND_VOSA"
            ? charter?.VosuThresholdPercent
            : null;

        var systemSettingKey = requestType.Code switch
        {
            "DEMAND_VOSU" => Domain.Constants.SystemSettingKeys.VosuDefaultThresholdPercent,
            "DEMAND_VOSA" => Domain.Constants.SystemSettingKeys.VosaDefaultThresholdPercent,
            _ => null
        };

        decimal? systemDefault = null;
        if (systemSettingKey is not null)
        {
            var setting = await ctx.SystemSettings.FirstOrDefaultAsync(x => x.Key == systemSettingKey, ct);
            if (setting is not null && decimal.TryParse(setting.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsed))
                systemDefault = parsed;
        }

        // Определяем: направлять ли требование СД (вместо ГД)
        bool isForBoard = charter?.BoardDecidesConveningOsu == true
            && requestType.ConsideredByOsu;

        // Определяем: хватает ли доли участнику для прямого направления
        var effectiveThreshold = charterThreshold ?? systemDefault;
        var participantShare = activeShare?.SharePercent ?? 0m;
        var hasEnoughShare = effectiveThreshold.HasValue && participantShare >= effectiveThreshold.Value;

        // Создаём запрос
        var entity = new ShareRequest
        {
            Id = Guid.NewGuid(),
            LegalEntityId = leId!.Value,
            ParticipantId = participant.Id,
            RequestTypeId = requestTypeId,
            Status = "draft",
            Text = text,
            Payload = payload,
            CreatedBy = userId,
            IsCollective = true,
            ThresholdPercent = effectiveThreshold,
            TotalSupportPercent = participantShare,
            SupporterCount = 1,
            CollectiveStatus = hasEnoughShare
                ? (isForBoard ? "BOARD_REVIEW" : "SUBMITTED_TO_CEO")
                : effectiveThreshold.HasValue
                    ? "COLLECTING"
                    : isForBoard ? "BOARD_REVIEW" : "SUBMITTED_TO_CEO"
        };

        // Автоматически добавляем поддержку инициатора
        var initiatorSupport = new ShareRequestSupport
        {
            Id = Guid.NewGuid(),
            ShareRequestId = entity.Id,
            ParticipantId = participant.Id,
            SharePercentAtSupport = activeShare?.SharePercent ?? 0m,
            SupportedAt = DateTime.UtcNow
        };

        if (!effectiveThreshold.HasValue)
            entity.SubmittedToCeoAt = DateTime.UtcNow;

        ctx.ShareRequests.Add(entity);
        ctx.ShareRequestSupports.Add(initiatorSupport);
        await ctx.SaveChangesAsync(ct);

        // Если требование сразу направлено ГД (достаточная доля) — уведомляем CEO и инициатора
        if (entity.CollectiveStatus == "SUBMITTED_TO_CEO")
        {
            entity.SubmittedToCeoAt ??= DateTime.UtcNow;
            await ctx.SaveChangesAsync(ct);
            await NotifyCeoAsync(ctx, entity, ct);
            await NotifyInitiatorAsync(ctx, entity, ct);
            await ctx.SaveChangesAsync(ct);
        }

        // Если требование направлено СД — создаём пункт повестки заседания СД
        if (entity.CollectiveStatus == "BOARD_REVIEW")
        {
            var activeBoard = await ctx.BoardsOfDirectors
                .Include(b => b.OsaMeeting)
                .Where(b => b.OsaMeeting!.LegalEntityId == leId && b.EndedAt == null)
                .OrderByDescending(b => b.ElectionYear)
                .FirstOrDefaultAsync(ct);
            if (activeBoard is not null)
            {
                var agendaItem = new AgendaItem
                {
                    Id = Guid.NewGuid(),
                    BoardOfDirectorsId = activeBoard.Id,
                    LegalEntityId = leId!.Value,
                    ShareRequestId = entity.Id,
                    Title = $"Требование: {requestType.Name}",
                    TargetType = "BOARD_MEETING",
                    Reason = "Требование участника (ст. 35 14-ФЗ)",
                    Status = "PENDING"
                };
                ctx.AgendaItems.Add(agendaItem);
                await ctx.SaveChangesAsync(ct);
            }
        }

        _logger.LogInformation("Создано коллективное требование {RequestId} типа {RequestTypeCode} пользователем {UserId}",
            entity.Id, requestType.Code, userId);
        return entity.Id;
    }

    /// <inheritdoc />
    public async Task<Guid> AddItemAsync(Guid requestId, string title, string? description, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var request = await ctx.ShareRequests.FindAsync(requestId);
        if (request is null)
            throw new InvalidOperationException("Требование не найдено");

        if (request.Status != "draft")
            throw new InvalidOperationException("Редактирование доступно только для черновиков");

        var maxSeq = await ctx.ShareRequestItems
            .Where(i => i.ShareRequestId == requestId)
            .MaxAsync(i => (int?)i.SequenceNumber, ct) ?? 0;

        var item = new ShareRequestItem
        {
            Id = Guid.NewGuid(),
            ShareRequestId = requestId,
            SequenceNumber = maxSeq + 1,
            Title = title,
            Description = description,
            Status = "pending"
        };

        ctx.ShareRequestItems.Add(item);
        await ctx.SaveChangesAsync(ct);

        _logger.LogInformation("Создан пункт {ItemId} для требования {RequestId}: {Title}", item.Id, requestId, title);
        return item.Id;
    }

    /// <inheritdoc />
    public async Task AttachFileToItemAsync(Guid requestId, Guid itemId, Guid fileId, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var item = await ctx.ShareRequestItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.ShareRequestId == requestId, ct);
        if (item is null)
            throw new InvalidOperationException("Пункт не найден");

        var fileExists = await ctx.Files.AnyAsync(f => f.Id == fileId, ct);
        if (!fileExists)
            throw new InvalidOperationException("Файл не найден");

        var alreadyAttached = await ctx.ShareRequestItemFiles
            .AnyAsync(f => f.ShareRequestItemId == itemId && f.FileId == fileId, ct);
        if (alreadyAttached)
            return; // Идемпотентность: файл уже прикреплён

        var fileLink = new ShareRequestItemFile
        {
            Id = Guid.NewGuid(),
            ShareRequestItemId = itemId,
            FileId = fileId
        };

        ctx.ShareRequestItemFiles.Add(fileLink);
        await ctx.SaveChangesAsync(ct);

        _logger.LogInformation("Файл {FileId} прикреплён к пункту {ItemId}", fileId, itemId);
    }

    /// <inheritdoc />
    public async Task AttachFileAsync(Guid requestId, Guid fileId, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var request = await ctx.ShareRequests.FindAsync(requestId);
        if (request is null)
            throw new InvalidOperationException("Запрос не найден");

        ValidateStatusForOperation(request, "attach_file");

        var fileEntry = await ctx.Files.FindAsync(fileId);
        if (fileEntry is null)
            throw new InvalidOperationException("Файл не найден");

        // Проверяем дубликат
        var exists = await ctx.ShareRequestFiles
            .AnyAsync(f => f.ShareRequestId == requestId && f.FileId == fileId, ct);
        if (exists)
            throw new InvalidOperationException("Файл уже прикреплён");

        var entity = new ShareRequestFile
        {
            Id = Guid.NewGuid(),
            ShareRequestId = requestId,
            FileId = fileId
        };

        ctx.ShareRequestFiles.Add(entity);
        await ctx.SaveChangesAsync(ct);

        _logger.LogInformation("Файл {FileId} прикреплён к требованию {RequestId}", fileId, requestId);
    }

    /// <inheritdoc />
    public async Task<int> GenerateVosuNotificationsAsync(Guid requestId, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var request = await ctx.ShareRequests
            .Include(r => r.RequestType)
            .FirstOrDefaultAsync(r => r.Id == requestId, ct);
        if (request is null)
            throw new InvalidOperationException("Требование не найдено");

        if (request.RequestType?.Code != "DEMAND_VOSU")
            throw new InvalidOperationException("Уведомления ВОСУ доступны только для типа DEMAND_VOSU");

        if (!request.OrgIntentId.HasValue)
            throw new InvalidOperationException("План ВОСУ не создан. Сначала примите требование.");

        var orgIntentId = request.OrgIntentId.Value;

        // Проверяем, нет ли уже сформированных уведомлений
        var existingCount = await ctx.VosuNotifications
            .CountAsync(n => n.OrgIntentId == orgIntentId, ct);
        if (existingCount > 0)
            throw new InvalidOperationException("Уведомления уже сформированы. Обновите страницу.");

        // Загружаем участников
        var participants = await ctx.BoardParticipants
            .Where(x => x.LegalEntityId == request.LegalEntityId && x.IsActive)
            .Include(x => x.EcosystemParticipant)
            .Include(x => x.Person)
            .ToListAsync(ct);
        if (participants.Count == 0)
            throw new InvalidOperationException("Нет активных участников для формирования уведомлений");

        // Юридическое лицо
        var legalEntity = await ctx.LegalEntities.FindAsync(request.LegalEntityId);
        if (legalEntity is null)
            throw new InvalidOperationException("Юридическое лицо не найдено");

        // ФИО инициатора
        string? initiatorName = null;
        decimal? initiatorSharePercent = null;

        // Получаем ФИО инициатора из участника
        var initiatorParticipant = await ctx.BoardParticipants
            .Include(p => p.Shares.Where(s => s.IsActive).Take(1))
            .FirstOrDefaultAsync(p => p.Id == request.ParticipantId, ct);
        if (initiatorParticipant is not null)
        {
            initiatorName = initiatorParticipant.ParticipantType == "FL"
                ? initiatorParticipant.Person?.FullName
                : initiatorParticipant.CompanyName;
            initiatorSharePercent = initiatorParticipant.Shares.FirstOrDefault()?.SharePercent;
        }

        // Данные для уведомлений берём из payload
        var meetingDate = DateOnly.FromDateTime(DateTime.UtcNow);
        TimeOnly? meetingStartTime = null;
        string? meetingVenue = null;
        TimeOnly? registrationStartTime = null;
        var agendaItems = new List<string>();
        string? reviewLocation = null;

        if (!string.IsNullOrEmpty(request.Payload))
        {
            try
            {
                var payload = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(request.Payload);
                if (payload.TryGetProperty("meetingDate", out var md) && md.ValueKind == System.Text.Json.JsonValueKind.String)
                    DateOnly.TryParse(md.GetString(), out meetingDate);
                if (payload.TryGetProperty("meetingStartTime", out var ms) && ms.ValueKind == System.Text.Json.JsonValueKind.String)
                    TimeOnly.TryParse(ms.GetString(), out var parsedMs);
                meetingStartTime = payload.TryGetProperty("meetingStartTime", out var ms2) && ms2.ValueKind == System.Text.Json.JsonValueKind.String && TimeOnly.TryParse(ms2.GetString(), out var parsedMs2) ? parsedMs2 : null;
                meetingVenue = payload.TryGetProperty("meetingVenue", out var mv) ? mv.GetString() : null;
                registrationStartTime = payload.TryGetProperty("registrationStartTime", out var rs) && rs.ValueKind == System.Text.Json.JsonValueKind.String && TimeOnly.TryParse(rs.GetString(), out var parsedRs) ? parsedRs : null;
                if (payload.TryGetProperty("agendaItems", out var ai) && ai.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    foreach (var item in ai.EnumerateArray())
                        if (item.ValueKind == System.Text.Json.JsonValueKind.String)
                            agendaItems.Add(item.GetString()!);
                }
                reviewLocation = payload.TryGetProperty("reviewLocation", out var rl) ? rl.GetString() : null;
            }
            catch
            {
                /* не JSON — используем значения по умолчанию */
            }
        }

        var notificationsCount = 0;

        foreach (var participant in participants)
        {
            var participantName = participant.ParticipantType == "FL"
                ? participant.Person?.FullName
                : participant.CompanyName;

            var docxData = new VosuNotificationData
            {
                LegalEntityName = legalEntity.Name,
                LegalEntityOgrn = legalEntity.Ogrn,
                LegalEntityInn = legalEntity.Inn,
                ParticipantFullName = participantName,
                ParticipantAddress = participant.ParticipantType == "FL"
                    ? (participant.PersonId.HasValue
                        ? ctx.IdentityDocuments.FirstOrDefault(x => x.PersonId == participant.PersonId.Value && x.IsActive)?.RegistrationAddress
                        : null)
                    : participant.CompanyAddress,
                MeetingDate = meetingDate,
                MeetingStartTime = meetingStartTime,
                MeetingVenue = meetingVenue,
                RegistrationStartTime = registrationStartTime,
                AgendaItems = agendaItems,
                InitiatorName = initiatorName,
                InitiatorSharePercent = initiatorSharePercent,
                DemandReceivedDate = DateOnly.FromDateTime(request.CreatedAt),
                ReviewLocation = reviewLocation,
                CeoName = "",
                NotificationDate = DateOnly.FromDateTime(DateTime.UtcNow),
                IsAgendaChange = false
            };

            var docxBytes = await _vosuDocxGenerator.GenerateAsync(docxData);
            using var ms = new MemoryStream(docxBytes);
            var sanitized = System.Text.RegularExpressions.Regex.Replace(participantName ?? "unknown", @"[^\w\-]", "_");
            var fileName = $"Уведомление_ВОСУ_{sanitized}.docx";

            var storageKey = await _fileStorage.SaveAsync(ms, fileName,
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
                CreatedBy = request.DecidedByUserId ?? request.CreatedBy
            };
            ctx.Files.Add(fileEntry);

            var vosuNotification = new VosuNotification
            {
                Id = Guid.NewGuid(),
                OrgIntentId = orgIntentId,
                BoardParticipantId = participant.Id,
                FileId = fileEntry.Id,
                CreatedBy = request.DecidedByUserId ?? request.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };
            ctx.VosuNotifications.Add(vosuNotification);

            notificationsCount++;
        }

        await ctx.SaveChangesAsync(ct);

        _logger.LogInformation("Сформировано уведомлений ВОСУ: {Count}, OrgIntentId: {OrgIntentId}",
            notificationsCount, orgIntentId);

        return notificationsCount;
    }

    // ── Private helpers ───────────────────────────────────────────

    /// <summary>Получить LegalEntityId по userId через EcosystemParticipants.</summary>
    private static async Task<Guid?> ResolveLegalEntityIdAsync(FiduciaDbContext ctx, Guid userId, CancellationToken ct = default)
    {
        var ep = await ctx.EcosystemParticipants.FirstOrDefaultAsync(x => x.UserId == userId, ct);
        return ep?.LegalEntityId;
    }

    /// <summary>Валидация допустимости операции по статусу требования.</summary>
    private static void ValidateStatusForOperation(ShareRequest request, string operation)
    {
        (bool Allowed, string? Error) result = operation switch
        {
            "update" => request.Status == "draft" || (request.IsCollective && request.CollectiveStatus == "COLLECTING")
                ? (true, null)
                : (false, "Редактирование доступно только для черновиков"),
            "submit" => request.Status == "draft"
                ? (true, null)
                : (false, "Требование уже отправлено"),
            "attach_file" => request.Status == "draft" || (request.IsCollective && request.CollectiveStatus == "COLLECTING")
                ? (true, null)
                : (false, "Прикрепление файлов недоступно для этого статуса"),
            "submit_decision" => (!request.IsCollective && request.Status == "submitted")
                || (request.IsCollective && (request.CollectiveStatus == "THRESHOLD_REACHED"
                    || request.CollectiveStatus == "SUBMITTED_TO_CEO"
                    || request.CollectiveStatus == "BOARD_REVIEW"
                    || request.CollectiveStatus == "OSU_REVIEW"))
                ? (true, null)
                : (false, "Требование не может быть рассмотрено в текущем статусе"),
            "revoke" => request.Status == "submitted"
                ? (true, null)
                : (false, "Отзыв доступен только для отправленных требований"),
            _ => (true, null)
        };

        if (!result.Allowed)
            throw new InvalidOperationException(result.Error);
    }

    private record DuplicateRequestResult(string Message, Guid PreviousRequestId);

    /// <summary>Специфичная валидация по типу запроса.</summary>
    private static async Task<DuplicateRequestResult?> ValidateRequestTypeAsync(
        FiduciaDbContext ctx, RefRequestType requestType, Guid leId, Guid participantId, string? payload, decimal? participantSharePercent = null, CancellationToken ct = default)
    {
        // Проверка: нет ли уже запроса того же типа от данного участника
        var existing = await ctx.ShareRequests
            .Include(r => r.OrgIntent)
                .ThenInclude(i => i!.Stages)
            .FirstOrDefaultAsync(r => r.LegalEntityId == leId
                && r.RequestTypeId == requestType.Id
                && r.ParticipantId == participantId, ct);
        if (existing is not null)
        {
            var deadline = existing.OrgIntent?.Stages
                .Where(s => s.PlannedEnd.HasValue)
                .Max(s => s.PlannedEnd);
            var deadlineStr = deadline?.ToString("dd.MM.yyyy") ?? "не установлен";
            var message = $"Уже есть активное требование типа «{requestType.Name}». Крайний срок: {deadlineStr}. [{existing.Id}]";
            return new DuplicateRequestResult(message, existing.Id);
        }

        var typeError = requestType.Code switch
        {
            "NOTARY_LIST_MAINTENANCE" => await ValidateNotaryListMaintenanceAsync(ctx, leId, ct),
            "PREEMPTIVE_LIST" => await ValidatePreemptiveListAsync(ctx, leId, ct),
            "EXIT_APPLICATION" => await ValidateExitApplicationAsync(ctx, leId, participantSharePercent, ct),
            "CHANGE_STANDARD_CHARTER_NUMBER" => await ValidateChangeStandardCharterNumberAsync(ctx, leId, payload, ct),
            "CONVERT_STANDARD_TO_CUSTOM_CHARTER" => await ValidateConvertToCustomCharterAsync(ctx, leId, payload, ct),
            "CHANGE_CUSTOM_CHARTER_PROVISION" => await ValidateChangeCustomCharterProvisionAsync(ctx, leId, ct),
            "DEMAND_VOSU" => await ValidateDemandVosuAsync(ctx, leId, payload, ct),
            "CONVERT_TO_NJSC" => await ValidateConvertToNjscAsync(ctx, leId, ct),
            "CONVERT_TO_PJSC" => await ValidateConvertToPjscAsync(ctx, leId, ct),
            "CHANGE_CHARTER_PROVISION" => await ValidateChangeCharterProvisionAsync(ctx, leId, ct),
            _ => null
        };

        return typeError is not null ? new DuplicateRequestResult(typeError, Guid.Empty) : null;
    }

    private static async Task<string?> ValidateNotaryListMaintenanceAsync(FiduciaDbContext ctx, Guid leId, CancellationToken ct)
    {
        var extraSettings = await ctx.LegalEntityExtraSettings
            .FirstOrDefaultAsync(x => x.LegalEntityId == leId, ct);
        if (extraSettings?.NotaryListApproved == true)
            return "Ведение списка участников через нотариат уже утверждено";
        return null;
    }

    private static async Task<string?> ValidatePreemptiveListAsync(FiduciaDbContext ctx, Guid leId, CancellationToken ct)
    {
        var charter = await ctx.LegalEntityCharters.FindAsync(leId);
        if (charter is not null && !charter.PreemptiveRight)
            return "Преимущественное право не действует";
        return null;
    }

    private static async Task<string?> ValidateExitApplicationAsync(FiduciaDbContext ctx, Guid leId, decimal? sharePercent, CancellationToken ct)
    {
        var charter = await ctx.LegalEntityCharters.FindAsync(leId);
        if (charter is not null && !charter.ExitAllowed)
            return "Выход из ООО не предусмотрен уставом";

        if (charter?.ExitAllowedMinSharePercent.HasValue == true && sharePercent.HasValue)
        {
            if (sharePercent.Value < charter.ExitAllowedMinSharePercent.Value)
                return $"Ваша доля ({sharePercent}%) ниже минимальной для выхода ({charter.ExitAllowedMinSharePercent}%)";
        }

        if (charter?.ExitAllowedMaxSharePercent.HasValue == true && sharePercent.HasValue)
        {
            if (sharePercent.Value > charter.ExitAllowedMaxSharePercent.Value)
                return $"Ваша доля ({sharePercent}%) выше максимальной для выхода ({charter.ExitAllowedMaxSharePercent}%)";
        }

        if (!string.IsNullOrEmpty(charter?.ExitConditionDescription))
            return $"Выход возможен при условии: {charter.ExitConditionDescription}";

        return null;
    }

    private static async Task<string?> ValidateChangeStandardCharterNumberAsync(FiduciaDbContext ctx, Guid leId, string? payload, CancellationToken ct)
    {
        var le = await ctx.LegalEntities.FirstOrDefaultAsync(x => x.Id == leId, ct);
        if (le?.StandardCharterId is null)
            return "Текущий устав не является типовым";

        if (!string.IsNullOrEmpty(payload))
        {
            var payloadJson = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(payload);
            if (payloadJson.TryGetProperty("newCharterNumber", out var newNum))
            {
                var currentCharter = await ctx.RefStandardCharters.FindAsync(le.StandardCharterId);
                if (currentCharter?.Number == newNum.GetString())
                    return "Новый номер типового устава должен отличаться от текущего";
            }
        }

        return null;
    }

    private static async Task<string?> ValidateConvertToCustomCharterAsync(FiduciaDbContext ctx, Guid leId, string? payload, CancellationToken ct)
    {
        var le = await ctx.LegalEntities.FirstOrDefaultAsync(x => x.Id == leId, ct);
        if (le?.StandardCharterId is null)
            return "Текущий устав уже является индивидуальным";

        if (string.IsNullOrEmpty(payload))
            return "Необходимо приложить файл проекта устава";

        return null;
    }

    private static async Task<string?> ValidateConvertToNjscAsync(FiduciaDbContext ctx, Guid leId, CancellationToken ct)
    {
        var le = await ctx.LegalEntities
            .Include(x => x.RefOkopf)
            .FirstOrDefaultAsync(x => x.Id == leId, ct);
        if (le?.RefOkopf?.Code is null)
            return "Не определён тип организации";
        if (!OkopfTypeMapper.IsLlc(le.RefOkopf.Code))
            return "Преобразование в НАО доступно только для ООО";
        return null;
    }

    private static async Task<string?> ValidateConvertToPjscAsync(FiduciaDbContext ctx, Guid leId, CancellationToken ct)
    {
        var le = await ctx.LegalEntities
            .Include(x => x.RefOkopf)
            .FirstOrDefaultAsync(x => x.Id == leId, ct);
        if (le?.RefOkopf?.Code is null)
            return "Не определён тип организации";
        if (le.RefOkopf.Code != OkopfTypeMapper.NjscCode)
            return "Преобразование в ПАО доступно только для НАО";
        return null;
    }

    private static async Task<string?> ValidateChangeCharterProvisionAsync(FiduciaDbContext ctx, Guid leId, CancellationToken ct)
    {
        var le = await ctx.LegalEntities.FirstOrDefaultAsync(x => x.Id == leId, ct);
        if (le?.StandardCharterId is not null)
            return "Устав является типовым; используйте требование «Изменить номер типового устава» или «Изменить типовой устав на индивидуальный»";
        return null;
    }

    private static async Task<string?> ValidateChangeCustomCharterProvisionAsync(FiduciaDbContext ctx, Guid leId, CancellationToken ct)
    {
        var le = await ctx.LegalEntities.FirstOrDefaultAsync(x => x.Id == leId, ct);
        if (le?.StandardCharterId is not null)
            return "Устав является типовым; используйте требование «Изменить номер типового устава»";
        return null;
    }

    private static async Task<string?> ValidateDemandVosuAsync(FiduciaDbContext ctx, Guid leId, string? payload, CancellationToken ct)
    {
        var charter = await ctx.LegalEntityCharters.FindAsync(leId);
        decimal? threshold = charter?.VosuThresholdPercent;
        if (threshold is null)
        {
            var setting = await ctx.SystemSettings.FirstOrDefaultAsync(x => x.Key == Domain.Constants.SystemSettingKeys.VosuDefaultThresholdPercent, ct);
            if (setting is not null && decimal.TryParse(setting.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsed))
                threshold = parsed;
        }

        decimal? sharePercent = null;
        if (!string.IsNullOrEmpty(payload))
        {
            var payloadJson = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(payload);
            if (payloadJson.TryGetProperty("sharePercent", out var sp))
                sharePercent = sp.GetDecimal();
        }

        if (sharePercent is null)
            return "Не указана доля участника";

        if (threshold.HasValue && sharePercent < threshold.Value)
            return $"Доля участника ({sharePercent}%) ниже порога ({threshold}%)";

        return null;
    }

    /// <summary>Уведомление CEO о поступлении требования.</summary>
    private static async Task NotifyCeoAsync(FiduciaDbContext ctx, ShareRequest request, CancellationToken ct = default)
    {
        var ceoUsers = await ctx.UserRoles
            .Include(ur => ur.User)
            .Where(ur => ur.Role.Code == "CEO")
            .Select(ur => ur.User!)
            .ToListAsync(ct);

        var isDirectDemand = !request.IsCollective
            || request.CollectiveStatus == "SUBMITTED_TO_CEO";

        foreach (var ceo in ceoUsers)
        {
            ctx.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = ceo.Id,
                NotificationType = "COLLECTIVE_DEMAND_THRESHOLD",
                Title = isDirectDemand
                    ? "Требование участника о созыве ВОСУ"
                    : "Коллективное требование набрало порог",
                Body = isDirectDemand
                    ? $"Участник подал требование о созыве ВОСУ ({request.TotalSupportPercent}% голосов). Требуется рассмотрение в течение 5 дней."
                    : $"Коллективное требование набрало {request.TotalSupportPercent}% (порог: {request.ThresholdPercent}%). Требуется рассмотрение.",
                Url = $"{DemandUrlPrefix}{request.Id}",
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    /// <summary>Уведомление инициатора о направлении требования ГД.</summary>
    private static async Task NotifyInitiatorAsync(FiduciaDbContext ctx, ShareRequest request, CancellationToken ct = default)
    {
        var initiatorUserId = await ctx.BoardParticipants
            .Where(p => p.Id == request.ParticipantId && p.EcosystemParticipantId != null)
            .Select(p => p.EcosystemParticipant!.UserId)
            .FirstOrDefaultAsync(ct);

        if (!initiatorUserId.HasValue)
            return;

        ctx.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = initiatorUserId.Value,
            NotificationType = "SHARE_REQUEST_VISIBLE_TO_ALL",
            Title = "Требование направлено ГД",
            Body = "Ваше требование о созыве ВОСУ направлено Генеральному директору на рассмотрение.",
            Url = $"/share-requests/{request.Id}/collective",
            CreatedAt = DateTime.UtcNow
        });
    }

    /// <summary>Уведомление всех поддержавших о решении.</summary>
    private static async Task NotifySupportersAsync(FiduciaDbContext ctx, ShareRequest request, CancellationToken ct = default)
    {
        var supporterIds = await ctx.ShareRequestSupports
            .Where(s => s.ShareRequestId == request.Id && s.WithdrawnAt == null)
            .Select(s => s.ParticipantId)
            .ToListAsync(ct);

        var ecoParticipantIds = await ctx.BoardParticipants
            .Where(p => supporterIds.Contains(p.Id) && p.EcosystemParticipantId != null)
            .Select(p => p.EcosystemParticipantId!.Value)
            .ToListAsync(ct);

        var userIds = await ctx.EcosystemParticipants
            .Where(p => ecoParticipantIds.Contains(p.Id) && p.UserId != null)
            .Select(p => p.UserId!.Value)
            .ToListAsync(ct);

        var decisionText = request.CollectiveStatus == "ACCEPTED" ? "принято" : "отклонено";
        var isCeoDecision = request.CollectiveStatus == "ACCEPTED" || request.CollectiveStatus == "REJECTED";
        var decisionBy = request.CollectiveStatus == "BOARD_REVIEW"
            ? "Совет директоров"
            : "Генеральный директор";
        var notificationTitle = isCeoDecision ? $"Решение ГД: {decisionText}" : $"Решение СД: {decisionText}";

        foreach (var userId in userIds)
        {
            ctx.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                NotificationType = "COLLECTIVE_DEMAND_DECISION",
                Title = notificationTitle,
                Body = $"{decisionBy} {decisionText} коллективное требование.",
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    /// <summary>Создание плана ВОСУ из шаблона.</summary>
    private async Task<Guid?> CreateVosuPlanAsync(FiduciaDbContext ctx, Guid legalEntityId, CancellationToken ct = default)
    {
        try
        {
            var taskCount = await _templateService.InstantiateAsync(ctx, "VOSU", legalEntityId, null);
            if (taskCount == 0)
            {
                _logger.LogWarning("Шаблон VOSU не найден или нет задач для ЮЛ {LegalEntityId}", legalEntityId);
                return null;
            }

            await ctx.SaveChangesAsync(ct);

            var orgIntent = await ctx.OrgIntents
                .Include(i => i.TemplateIntent)
                .Where(i => i.LegalEntityId == legalEntityId
                    && i.TemplateIntent!.Code == "VOSU")
                .OrderByDescending(i => i.CreatedAt)
                .FirstOrDefaultAsync(ct);

            if (orgIntent != null)
                _logger.LogInformation("Создан план ВОСУ {OrgIntentId} для ЮЛ {LegalEntityId}, задач: {TaskCount}",
                    orgIntent.Id, legalEntityId, taskCount);

            return orgIntent?.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка создания плана ВОСУ для ЮЛ {LegalEntityId}", legalEntityId);
            return null;
        }
    }

    /// <summary>Создание плана ВОСА из шаблона.</summary>
    private async Task<Guid?> CreateVosaPlanAsync(FiduciaDbContext ctx, Guid legalEntityId, CancellationToken ct = default)
    {
        try
        {
            var taskCount = await _templateService.InstantiateAsync(ctx, "VOSA", legalEntityId, null);
            if (taskCount == 0)
            {
                _logger.LogWarning("Шаблон VOSA не найден или нет задач для ЮЛ {LegalEntityId}", legalEntityId);
                return null;
            }

            await ctx.SaveChangesAsync(ct);

            var orgIntent = await ctx.OrgIntents
                .Include(i => i.TemplateIntent)
                .Where(i => i.LegalEntityId == legalEntityId
                    && i.TemplateIntent!.Code == "VOSA")
                .OrderByDescending(i => i.CreatedAt)
                .FirstOrDefaultAsync(ct);

            if (orgIntent != null)
                _logger.LogInformation("Создан план ВОСА {OrgIntentId} для ЮЛ {LegalEntityId}, задач: {TaskCount}",
                    orgIntent.Id, legalEntityId, taskCount);

            return orgIntent?.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка создания плана ВОСА для ЮЛ {LegalEntityId}", legalEntityId);
            return null;
        }
    }
}
