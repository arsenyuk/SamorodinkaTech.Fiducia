using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Validation;
using SamorodinkaTech.Fiducia.Infrastructure;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.BoardPortal;

/// <summary>
/// Minimal API endpoints для запросов участника в общество (Board Portal).
/// Типы берутся из справочника ref_request_type.
/// </summary>
public static class ShareRequestEndpoints
{
    private const string AuditActionAccess = "SHARE_REQUEST_ACCESS";

    public static void MapShareRequestEndpoints(this WebApplication app)
    {
        var shareRequests = app.MapGroup("/api/share-requests")
            .RequireAuthorization()
            .WithTags("Share Requests");

        // GET: справочник типов требований (доступных текущему ЮЛ)
        shareRequests.MapGet("/types", async (
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ILoggerFactory loggerFactory,
            HttpContext http) =>
        {
            var logger = loggerFactory.CreateLogger("ShareRequests.Types");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var leId = await GetLegalEntityIdAsync(ctx);
                if (leId is null)
                    return Results.Ok(Array.Empty<object>());

                var le = await ctx.LegalEntities
                    .Include(x => x.RefOkopf)
                    .FirstOrDefaultAsync(x => x.Id == leId.Value);

                var okopfCode = le?.RefOkopf?.Code;
                var isLlc = OkopfTypeMapper.IsLlc(okopfCode);
                var isNjsc = okopfCode == OkopfTypeMapper.NjscCode;
                var isPjsc = OkopfTypeMapper.IsPjsc(okopfCode);

                var charter = await ctx.LegalEntityCharters.FindAsync(leId);
                var vosuThreshold = charter?.VosuThresholdPercent;

                var extraSettings = await ctx.LegalEntityExtraSettings
                    .FirstOrDefaultAsync(x => x.LegalEntityId == leId);

                var types = await ctx.RequestTypes
                    .Where(t => (isLlc && t.IsForLlc) || (isNjsc && t.IsForNjsc) || (isPjsc && t.IsForPjsc))
                    .OrderBy(t => t.Name)
                    .Select(t => new { t.Id, t.Code, t.Name, t.RequiresFile })
                    .ToListAsync();

                // Обогащаем информацией о пороге, правовой норме и доступности
                var enrichedTypes = types.Select(t => new
                {
                    t.Id,
                    t.Code,
                    t.Name,
                    t.RequiresFile,
                    RequiresThreshold = t.Code is "DEMAND_VOSU" or "DEMAND_VOSA",
                    ThresholdPercent = t.Code is "DEMAND_VOSU" or "DEMAND_VOSA" ? vosuThreshold : null,
                    LegalBasis = GetLegalBasisCode(t.Code),
                    IsCollective = IsCollectiveTypeCode(t.Code),
                    IsAvailable = t.Code switch
                    {
                        "EXIT_APPLICATION" => charter?.ExitAllowed ?? false,
                        "PREEMPTIVE_LIST" => charter?.PreemptiveRight ?? true,
                        "NOTARY_LIST_MAINTENANCE" => !(extraSettings?.NotaryListApproved ?? false),
                        _ => true
                    },
                    UnavailabilityReason = t.Code switch
                    {
                        "EXIT_APPLICATION" when !(charter?.ExitAllowed ?? false) => "Выход из общества не предусмотрен уставом",
                        "PREEMPTIVE_LIST" when !(charter?.PreemptiveRight ?? true) => "Преимущественное право не действует",
                        "NOTARY_LIST_MAINTENANCE" when extraSettings?.NotaryListApproved == true => "Ведение списка через нотариат уже утверждено",
                        _ => null
                    }
                });

                return Results.Ok(enrichedTypes);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка получения типов требований");
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // GET: порог для типа требования
        shareRequests.MapGet("/threshold", async (
            string typeCode,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("ShareRequests.Threshold");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var leId = await GetLegalEntityIdAsync(ctx);
                if (leId is null) return Results.Ok(new { threshold = (decimal?)null, defaultThreshold = 10m });

                decimal? threshold = typeCode switch
                {
                    "DEMAND_VOSU" or "DEMAND_VOSA" =>
                        (await ctx.LegalEntityCharters.FindAsync(leId))?.VosuThresholdPercent,
                    _ => null
                };

                return Results.Ok(new { threshold, defaultThreshold = 10m });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка получения порога для типа {TypeCode}", typeCode);
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // GET: список запросов текущего участника
        shareRequests.MapGet("/", async (
            Guid? typeId,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            HttpContext http) =>
        {
            var logger = loggerFactory.CreateLogger("ShareRequests.List");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var (leId, error) = await ValidateAccessAsync(ctx, http, audit);
                if (error is not null) return error;

                var query = ctx.ShareRequests
                    .Include(r => r.RequestType)
                    .Where(r => r.LegalEntityId == leId);

                if (typeId.HasValue)
                    query = query.Where(r => r.RequestTypeId == typeId.Value);

                var items = await query
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                return Results.Ok(items.Select(MapToDto));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка получения списка запросов");
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // GET: один запрос по ID
        shareRequests.MapGet("/{id}", async (
            Guid id,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            HttpContext http) =>
        {
            var logger = loggerFactory.CreateLogger("ShareRequests.Get");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var (leId, error) = await ValidateAccessAsync(ctx, http, audit);
                if (error is not null) return error;

                var item = await ctx.ShareRequests
                    .Include(r => r.RequestType)
                    .FirstOrDefaultAsync(r => r.Id == id && r.LegalEntityId == leId);

                return item is null
                    ? Results.NotFound()
                    : Results.Ok(MapToDto(item));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка получения запроса {Id}", id);
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // POST: создать запрос
        shareRequests.MapPost("/", async (
            ShareRequestCreateDto dto,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            HttpContext http) =>
        {
            var logger = loggerFactory.CreateLogger("ShareRequests.Create");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var (leId, error) = await ValidateAccessAsync(ctx, http, audit);
                if (error is not null) return error;

                // Загружаем тип запроса из справочника
                var requestType = await ctx.RequestTypes.FindAsync(dto.RequestTypeId);
                if (requestType is null)
                    return Results.BadRequest(new { error = $"Неизвестный тип запроса: {dto.RequestTypeId}" });

                // Проверяем доступность типа для текущего ЮЛ
                var le = await ctx.LegalEntities
                    .Include(x => x.RefOkopf)
                    .FirstOrDefaultAsync(x => x.Id == leId);
                var okopfCode = le?.RefOkopf?.Code;
                var isLlc = OkopfTypeMapper.IsLlc(okopfCode);
                var isNjsc = okopfCode == OkopfTypeMapper.NjscCode;
                var isPjsc = OkopfTypeMapper.IsPjsc(okopfCode);

                if ((isLlc && !requestType.IsForLlc) || (isNjsc && !requestType.IsForNjsc) || (isPjsc && !requestType.IsForPjsc))
                    return Results.BadRequest(new { error = $"Тип запроса «{requestType.Name}» не доступен для данного типа организации" });

                // Специфичная валидация по типам
                var validationError = await ValidateRequestTypeAsync(ctx, requestType, leId.Value, dto);
                if (validationError is not null)
                    return Results.BadRequest(new { error = validationError });

                var userIdStr = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var createdBy = Guid.TryParse(userIdStr, out var uid) ? uid : Guid.Empty;

                // Находим участника: user → person → person.id = participant.person_id
                var user = await ctx.Users.FindAsync(createdBy);
                if (user?.PersonId is null)
                    return Results.BadRequest(new { error = "Пользователь не привязан к физическому лицу" });

                var participant = await ctx.BoardParticipants
                    .FirstOrDefaultAsync(p => p.LegalEntityId == leId.Value && p.PersonId == user.PersonId && p.IsActive);
                if (participant is null)
                    return Results.BadRequest(new { error = "Не найден участник для текущего пользователя" });

                var entity = new ShareRequest
                {
                    Id = Guid.NewGuid(),
                    LegalEntityId = leId.Value,
                    ParticipantId = participant.Id,
                    RequestTypeId = dto.RequestTypeId,
                    Status = "draft",
                    Payload = dto.Payload,
                    CreatedBy = createdBy
                };

                ctx.ShareRequests.Add(entity);
                await ctx.SaveChangesAsync();

                return Results.Ok(MapToDto(entity));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка создания запроса");
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // POST: завершить запрос (администрация)
        shareRequests.MapPost("/{id}/complete", async (
            Guid id,
            ShareRequestCompleteDto dto,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            HttpContext http) =>
        {
            var logger = loggerFactory.CreateLogger("ShareRequests.Complete");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var (leId, error) = await ValidateAccessAsync(ctx, http, audit);
                if (error is not null) return error;

                var item = await ctx.ShareRequests
                    .FirstOrDefaultAsync(r => r.Id == id && r.LegalEntityId == leId);

                if (item is null)
                    return Results.NotFound();

                var (allowed, statusError) = ValidateStatusForOperation(item, "submit_decision");
                if (!allowed)
                    return Results.BadRequest(new { error = statusError });

                item.Status = dto.Status ?? "completed";
                item.CompletedAt = DateTime.UtcNow;
                if (dto.Payload is not null)
                    item.Payload = dto.Payload;

                await ctx.SaveChangesAsync();

                return Results.Ok(MapToDto(item));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка завершения запроса {Id}", id);
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // PUT: обновить черновик требования
        shareRequests.MapPut("/{id}", async (
            Guid id,
            ShareRequestUpdateDto dto,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            HttpContext http) =>
        {
            var logger = loggerFactory.CreateLogger("ShareRequests.Update");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var (leId, error) = await ValidateAccessAsync(ctx, http, audit);
                if (error is not null) return error;

                var item = await ctx.ShareRequests
                    .FirstOrDefaultAsync(r => r.Id == id && r.LegalEntityId == leId);

                if (item is null)
                    return Results.NotFound();

                var (allowed, statusError) = ValidateStatusForOperation(item, "update");
                if (!allowed)
                    return Results.BadRequest(new { error = statusError });

                if (dto.Payload is not null)
                    item.Payload = dto.Payload;

                await ctx.SaveChangesAsync();

                return Results.Ok(MapToDto(item));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка обновления запроса {Id}", id);
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // POST: отправить требование (перевод из draft в submitted)
        shareRequests.MapPost("/{id}/submit", async (
            Guid id,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            HttpContext http) =>
        {
            var logger = loggerFactory.CreateLogger("ShareRequests.Submit");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var (leId, error) = await ValidateAccessAsync(ctx, http, audit);
                if (error is not null) return error;

                var item = await ctx.ShareRequests
                    .FirstOrDefaultAsync(r => r.Id == id && r.LegalEntityId == leId);

                if (item is null)
                    return Results.NotFound();

                var (allowed, statusError) = ValidateStatusForOperation(item, "submit");
                if (!allowed)
                    return Results.BadRequest(new { error = statusError });

                item.Status = "submitted";

                await ctx.SaveChangesAsync();

                return Results.Ok(MapToDto(item));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка отправки запроса {Id}", id);
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // POST: отозвать запрос (только NOTARIAL_OFFER, в течение 24ч)
        shareRequests.MapPost("/{id}/revoke", async (
            Guid id,
            ShareRequestRevokeDto dto,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            HttpContext http) =>
        {
            var logger = loggerFactory.CreateLogger("ShareRequests.Revoke");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var (leId, error) = await ValidateAccessAsync(ctx, http, audit);
                if (error is not null) return error;

                var item = await ctx.ShareRequests
                    .Include(r => r.RequestType)
                    .FirstOrDefaultAsync(r => r.Id == id && r.LegalEntityId == leId);

                if (item is null)
                    return Results.NotFound();

                if (item.RequestType?.Code != "NOTARIAL_OFFER")
                    return Results.BadRequest(new { error = "Отзыв доступен только для нотариальных оферт" });

                var (allowed, statusError) = ValidateStatusForOperation(item, "revoke");
                if (!allowed)
                    return Results.BadRequest(new { error = statusError });

                if (item.RevokedAt.HasValue)
                    return Results.BadRequest(new { error = "Запрос уже отозван" });

                if ((DateTime.UtcNow - item.CreatedAt).TotalHours > 24)
                    return Results.BadRequest(new { error = "Прошло более 24 часов с момента создания" });

                item.Status = "revoked";
                item.RevokedAt = DateTime.UtcNow;
                item.RevokedByNotarized = dto.Notarized;

                await ctx.SaveChangesAsync();

                return Results.Ok(MapToDto(item));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка отзыва запроса {Id}", id);
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // GET: результат запроса (PREEMPTIVE_LIST — список участников)
        shareRequests.MapGet("/{id}/result", async (
            Guid id,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            HttpContext http) =>
        {
            var logger = loggerFactory.CreateLogger("ShareRequests.Result");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var (leId, error) = await ValidateAccessAsync(ctx, http, audit);
                if (error is not null) return error;

                var item = await ctx.ShareRequests
                    .Include(r => r.RequestType)
                    .FirstOrDefaultAsync(r => r.Id == id && r.LegalEntityId == leId);

                if (item is null)
                    return Results.NotFound();

                if (item.RequestType?.Code != "PREEMPTIVE_LIST")
                    return Results.BadRequest(new { error = "Результат доступен только для запроса списка участников" });

                if (item.Status != "completed")
                    return Results.BadRequest(new { error = "Запрос ещё не завершён" });

                var participants = await ctx.BoardParticipants
                    .Where(p => p.LegalEntityId == leId && p.IsActive)
                    .Select(p => new { p.FullName, p.CompanyName, p.CompanyInn, p.ParticipantType, p.SharePercent, p.ShareAmount })
                    .ToListAsync();

                return Results.Ok(participants);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка получения результата запроса {Id}", id);
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // GET: входящие оферты (видимые всем)
        shareRequests.MapGet("/incoming", async (
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            HttpContext http) =>
        {
            var logger = loggerFactory.CreateLogger("ShareRequests.Incoming");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var (leId, error) = await ValidateAccessAsync(ctx, http, audit);
                if (error is not null) return error;

                var items = await ctx.ShareRequests
                    .Include(r => r.RequestType)
                    .Where(r => r.LegalEntityId == leId
                        && r.VisibleToAll
                        && r.RequestType!.Code == "NOTARIAL_OFFER"
                        && r.Status != "revoked")
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                return Results.Ok(items.Select(MapToDto));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка получения входящих оферт");
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // ── Коллективные требования ───────────────────────────────────

        // POST: создать коллективное требование
        shareRequests.MapPost("/collective", async (
            ShareRequestCollectiveCreateDto dto,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            HttpContext http) =>
        {
            var logger = loggerFactory.CreateLogger("ShareRequests.CreateCollective");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var (leId, error) = await ValidateAccessAsync(ctx, http, audit);
                if (error is not null) return error;

                // Находим текущего участника
                var userIdStr = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var createdBy = Guid.TryParse(userIdStr, out var uid) ? uid : Guid.Empty;

                var user = await ctx.Users.FindAsync(createdBy);
                if (user?.PersonId is null)
                    return Results.BadRequest(new { error = "Пользователь не привязан к физическому лицу" });

                var participant = await ctx.BoardParticipants
                    .FirstOrDefaultAsync(p => p.LegalEntityId == leId && p.PersonId == user.PersonId && p.IsActive);
                if (participant is null)
                    return Results.BadRequest(new { error = "Не найден участник для текущего пользователя" });

                // Проверяем тип требования
                var requestType = await ctx.RequestTypes.FindAsync(dto.RequestTypeId);
                if (requestType is null)
                    return Results.BadRequest(new { error = $"Неизвестный тип требования: {dto.RequestTypeId}" });

                var le = await ctx.LegalEntities
                    .Include(x => x.RefOkopf)
                    .FirstOrDefaultAsync(x => x.Id == leId);
                var okopfCode = le?.RefOkopf?.Code;
                var isLlc = OkopfTypeMapper.IsLlc(okopfCode);
                var isNjsc = okopfCode == OkopfTypeMapper.NjscCode;
                var isPjsc = OkopfTypeMapper.IsPjsc(okopfCode);

                if ((isLlc && !requestType.IsForLlc) || (isNjsc && !requestType.IsForNjsc) || (isPjsc && !requestType.IsForPjsc))
                    return Results.BadRequest(new { error = $"Тип требования «{requestType.Name}» не доступен для данного типа организации" });

                // Определяем порог по типу запроса (ст. 35 14-ФЗ / ст. 55 208-ФЗ)
                var charter = await ctx.LegalEntityCharters.FindAsync(leId);
                decimal? threshold = requestType.Code switch
                {
                    "DEMAND_VOSU" or "DEMAND_VOSA" => charter?.VosuThresholdPercent,
                    // ADD_AGENDA_OSU, ADD_AGENDA_GOSA — без порога по закону
                    _ => null
                };

                // Создаём запрос
                var entity = new ShareRequest
                {
                    Id = Guid.NewGuid(),
                    LegalEntityId = leId!.Value,
                    ParticipantId = participant.Id,
                    RequestTypeId = dto.RequestTypeId,
                    Status = "draft",
                    Payload = dto.Payload,
                    CreatedBy = createdBy,
                    IsCollective = true,
                    ThresholdPercent = threshold,
                    TotalSupportPercent = participant.SharePercent ?? 0m,
                    SupporterCount = 1,
                    CollectiveStatus = threshold.HasValue ? "COLLECTING" : "SUBMITTED_TO_CEO"
                };

                // Автоматически добавляем поддержку инициатора
                var initiatorSupport = new ShareRequestSupport
                {
                    Id = Guid.NewGuid(),
                    ShareRequestId = entity.Id,
                    ParticipantId = participant.Id,
                    SharePercentAtSupport = participant.SharePercent ?? 0m,
                    SupportedAt = DateTime.UtcNow
                };

                if (!threshold.HasValue)
                    entity.SubmittedToCeoAt = DateTime.UtcNow;

                ctx.ShareRequests.Add(entity);
                ctx.ShareRequestSupports.Add(initiatorSupport);
                await ctx.SaveChangesAsync();

                return Results.Ok(MapToDto(entity));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка создания коллективного требования");
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // POST: поддержать коллективное требование
        shareRequests.MapPost("/{id}/support", async (
            Guid id,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            HttpContext http) =>
        {
            var logger = loggerFactory.CreateLogger("ShareRequests.Support");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var (leId, error) = await ValidateAccessAsync(ctx, http, audit);
                if (error is not null) return error;

                var request = await ctx.ShareRequests.FindAsync(id);
                if (request is null || request.LegalEntityId != leId || !request.IsCollective)
                    return Results.NotFound();

                if (request.CollectiveStatus != "COLLECTING")
                    return Results.BadRequest(new { error = "Поддержка доступна только для требований в статусе «Сбор поддержек»" });

                // Находим текущего участника
                var userIdStr = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var userId = Guid.TryParse(userIdStr, out var uid) ? uid : Guid.Empty;

                var user = await ctx.Users.FindAsync(userId);
                if (user?.PersonId is null)
                    return Results.BadRequest(new { error = "Пользователь не привязан к физическому лицу" });

                var participant = await ctx.BoardParticipants
                    .FirstOrDefaultAsync(p => p.LegalEntityId == leId && p.PersonId == user.PersonId && p.IsActive);
                if (participant is null)
                    return Results.BadRequest(new { error = "Не найден участник для текущего пользователя" });

                // Проверяем: не поддерживал ли уже
                var existingSupport = await ctx.ShareRequestSupports
                    .FirstOrDefaultAsync(s => s.ShareRequestId == id && s.ParticipantId == participant.Id && s.WithdrawnAt == null);
                if (existingSupport is not null)
                    return Results.BadRequest(new { error = "Вы уже поддержали это требование" });

                // Добавляем поддержку
                var support = new ShareRequestSupport
                {
                    Id = Guid.NewGuid(),
                    ShareRequestId = id,
                    ParticipantId = participant.Id,
                    SharePercentAtSupport = participant.SharePercent ?? 0m,
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
                    request.CollectiveStatus = "THRESHOLD_REACHED";
                    await NotifyCeoAsync(ctx, request, logger);
                }

                await ctx.SaveChangesAsync();

                return Results.Ok(new { request.TotalSupportPercent, request.SupporterCount, request.CollectiveStatus });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка поддержки требования {Id}", id);
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // POST: отозвать поддержку
        shareRequests.MapPost("/{id}/withdraw", async (
            Guid id,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            HttpContext http) =>
        {
            var logger = loggerFactory.CreateLogger("ShareRequests.Withdraw");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var (leId, error) = await ValidateAccessAsync(ctx, http, audit);
                if (error is not null) return error;

                var request = await ctx.ShareRequests.FindAsync(id);
                if (request is null || request.LegalEntityId != leId || !request.IsCollective)
                    return Results.NotFound();

                if (request.CollectiveStatus != "COLLECTING")
                    return Results.BadRequest(new { error = "Отзыв доступен только для требований в статусе «Сбор поддержек»" });

                // Находим текущего участника
                var userIdStr = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var userId = Guid.TryParse(userIdStr, out var uid) ? uid : Guid.Empty;

                var user = await ctx.Users.FindAsync(userId);
                if (user?.PersonId is null)
                    return Results.BadRequest(new { error = "Пользователь не привязан к физическому лицу" });

                var participant = await ctx.BoardParticipants
                    .FirstOrDefaultAsync(p => p.LegalEntityId == leId && p.PersonId == user.PersonId && p.IsActive);
                if (participant is null)
                    return Results.BadRequest(new { error = "Не найден участник для текущего пользователя" });

                var support = await ctx.ShareRequestSupports
                    .FirstOrDefaultAsync(s => s.ShareRequestId == id && s.ParticipantId == participant.Id && s.WithdrawnAt == null);
                if (support is null)
                    return Results.BadRequest(new { error = "Вы не поддерживали это требование" });

                // Отзываем поддержку
                support.WithdrawnAt = DateTime.UtcNow;

                // Пересчитываем суммарную долю
                request.TotalSupportPercent -= support.SharePercentAtSupport;
                request.SupporterCount -= 1;

                await ctx.SaveChangesAsync();

                return Results.Ok(new { request.TotalSupportPercent, request.SupporterCount, request.CollectiveStatus });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка отзыва поддержки {Id}", id);
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // POST: решение ГД
        shareRequests.MapPost("/{id}/decide", async (
            Guid id,
            ShareRequestDecideDto dto,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            HttpContext http) =>
        {
            var logger = loggerFactory.CreateLogger("ShareRequests.Decide");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var (leId, error) = await ValidateAccessAsync(ctx, http, audit);
                if (error is not null) return error;

                // Проверяем роль CEO
                var userIdStr = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var userId = Guid.TryParse(userIdStr, out var uid) ? uid : Guid.Empty;

                var user = await ctx.Users
                    .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == userId);
                var isCeo = user?.UserRoles?.Any(ur => ur.Role?.Code == "CEO") ?? false;
                if (!isCeo)
                    return Results.Forbid();

                var request = await ctx.ShareRequests.FindAsync(id);
                if (request is null || request.LegalEntityId != leId)
                    return Results.NotFound();

                var (allowed, statusError) = ValidateStatusForOperation(request, "submit_decision");
                if (!allowed)
                    return Results.BadRequest(new { error = statusError });

                if (dto.Decision != "ACCEPTED" && dto.Decision != "REJECTED")
                    return Results.BadRequest(new { error = "Решение должно быть ACCEPTED или REJECTED" });

                if (request.IsCollective)
                {
                    request.CollectiveStatus = dto.Decision;
                }
                else
                {
                    request.Status = dto.Decision;
                }
                request.CeoComment = dto.Comment;
                request.CeoDecisionAt = DateTime.UtcNow;
                request.DecidedByUserId = userId;
                request.CompletedAt = DateTime.UtcNow;
                request.SubmittedToCeoAt ??= DateTime.UtcNow;

                await ctx.SaveChangesAsync();

                // Если требование принято и тип = DEMAND_VOSU — создаём план ВОСУ
                if (dto.Decision == "ACCEPTED" && request.RequestType?.Code == "DEMAND_VOSU")
                {
                    var templateService = http.RequestServices.GetRequiredService<ITemplateInstantiationService>();
                    var orgIntentId = await CreateVosuPlanAsync(ctx, templateService, request.LegalEntityId, logger);
                    if (orgIntentId.HasValue)
                    {
                        request.OrgIntentId = orgIntentId.Value;
                        await ctx.SaveChangesAsync();
                    }
                }

                // Если требование принято и тип = DEMAND_VOSA — создаём план ВОСА
                if (dto.Decision == "ACCEPTED" && request.RequestType?.Code == "DEMAND_VOSA")
                {
                    var templateService = http.RequestServices.GetRequiredService<ITemplateInstantiationService>();
                    var orgIntentId = await CreateVosaPlanAsync(ctx, templateService, request.LegalEntityId, logger);
                    if (orgIntentId.HasValue)
                    {
                        request.OrgIntentId = orgIntentId.Value;
                        await ctx.SaveChangesAsync();
                    }
                }

                // Уведомляем всех поддержавших
                await NotifySupportersAsync(ctx, request, logger);

                return Results.Ok(MapToDto(request));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка решения по требованию {Id}", id);
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // GET: список коллективных требований
        shareRequests.MapGet("/collective", async (
            string? status,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            HttpContext http) =>
        {
            var logger = loggerFactory.CreateLogger("ShareRequests.ListCollective");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var (leId, error) = await ValidateAccessAsync(ctx, http, audit);
                if (error is not null) return error;

                var query = ctx.ShareRequests
                    .Include(r => r.RequestType)
                    .Include(r => r.Participant)
                    .Where(r => r.LegalEntityId == leId && r.IsCollective);

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(r => r.CollectiveStatus == status);

                var items = await query
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                return Results.Ok(items.Select(MapToCollectiveDto));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка получения списка коллективных требований");
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // GET: требования на рассмотрении ГД
        shareRequests.MapGet("/ceo-review", async (
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            HttpContext http) =>
        {
            var logger = loggerFactory.CreateLogger("ShareRequests.CeoReview");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var (leId, error) = await ValidateAccessAsync(ctx, http, audit);
                if (error is not null) return error;

                // Проверяем роль CEO
                var userIdStr = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var userId = Guid.TryParse(userIdStr, out var uid) ? uid : Guid.Empty;
                var user = await ctx.Users
                    .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == userId);
                var isCeo = user?.UserRoles?.Any(ur => ur.Role?.Code == "CEO") ?? false;
                if (!isCeo)
                    return Results.Forbid();

                // ГД видит требования, которые:
                // 1. Достигли порога (collective_status = "THRESHOLD_REACHED")
                // 2. ИЛИ когда есть активная ОСУ (any OsaMeeting with Status = "DRAFT")
                var hasActiveOsa = await ctx.OsaMeetings.AnyAsync(m => m.LegalEntityId == leId && m.Status == "DRAFT");

                var query = ctx.ShareRequests
                    .Include(r => r.RequestType)
                    .Include(r => r.Participant)
                    .Where(r => r.LegalEntityId == leId);

                if (hasActiveOsa)
                {
                    // В окно ОСУ — все не завершённые коллективные требования + все submitted одиночные
                    query = query.Where(r => (r.IsCollective && r.CollectiveStatus != "ACCEPTED" && r.CollectiveStatus != "REJECTED")
                        || (!r.IsCollective && r.Status == "submitted"));
                }
                else
                {
                    // Только достигшие порога коллективные + все submitted одиночные
                    query = query.Where(r => (r.IsCollective && r.CollectiveStatus == "THRESHOLD_REACHED")
                        || (!r.IsCollective && r.Status == "submitted"));
                }

                var items = await query
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                return Results.Ok(items.Select(MapToCollectiveDto));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка получения требований для ГД");
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // GET: список поддержавших
        shareRequests.MapGet("/{id}/supports", async (
            Guid id,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            HttpContext http) =>
        {
            var logger = loggerFactory.CreateLogger("ShareRequests.Supports");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var (leId, error) = await ValidateAccessAsync(ctx, http, audit);
                if (error is not null) return error;

                var request = await ctx.ShareRequests.FirstOrDefaultAsync(r => r.Id == id && r.LegalEntityId == leId);
                if (request is null)
                    return Results.NotFound();

                var userIdStr = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var userId = Guid.TryParse(userIdStr, out var uid) ? uid : Guid.Empty;
                var currentUser = await ctx.Users.FindAsync(userId);
                var currentParticipant = currentUser?.PersonId is not null
                    ? await ctx.BoardParticipants.FirstOrDefaultAsync(p => p.LegalEntityId == leId && p.PersonId == currentUser.PersonId && p.IsActive)
                    : null;

                var supports = await ctx.ShareRequestSupports
                    .Include(s => s.Participant)
                    .Where(s => s.ShareRequestId == id)
                    .OrderByDescending(s => s.SupportedAt)
                    .ToListAsync();

                return Results.Ok(supports.Select(s => new
                {
                    s.Id,
                    ParticipantName = s.Participant?.FullName ?? s.Participant?.CompanyName,
                    s.SharePercentAtSupport,
                    s.SupportedAt,
                    s.WithdrawnAt,
                    IsCurrentUser = s.ParticipantId == currentParticipant?.Id,
                    IsActive = s.WithdrawnAt == null
                }));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка получения поддержек {Id}", id);
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // ── Файлы требований ──────────────────────────────────────────

        // POST: прикрепить файл к требованию
        shareRequests.MapPost("/{id}/files", async (
            Guid id,
            ShareRequestAttachFileDto dto,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            HttpContext http) =>
        {
            var logger = loggerFactory.CreateLogger("ShareRequests.AttachFile");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var (leId, error) = await ValidateAccessAsync(ctx, http, audit);
                if (error is not null) return error;

                var request = await ctx.ShareRequests.FindAsync(id);
                if (request is null || request.LegalEntityId != leId)
                    return Results.NotFound();

                var (allowed, statusError) = ValidateStatusForOperation(request, "attach_file");
                if (!allowed)
                    return Results.BadRequest(new { error = statusError });

                var fileEntry = await ctx.Files.FindAsync(dto.FileId);
                if (fileEntry is null)
                    return Results.BadRequest(new { error = "Файл не найден" });

                // Проверяем дубликат
                var exists = await ctx.ShareRequestFiles
                    .AnyAsync(f => f.ShareRequestId == id && f.FileId == dto.FileId);
                if (exists)
                    return Results.BadRequest(new { error = "Файл уже прикреплён" });

                var entity = new ShareRequestFile
                {
                    Id = Guid.NewGuid(),
                    ShareRequestId = id,
                    FileId = dto.FileId
                };

                ctx.ShareRequestFiles.Add(entity);
                await ctx.SaveChangesAsync();

                logger.LogInformation("Файл {FileId} прикреплён к требованию {RequestId}", dto.FileId, id);

                return Results.Ok(new { entity.Id, entity.FileId, fileEntry.OriginalName });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка прикрепления файла к требованию {Id}", id);
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // GET: список файлов требования
        shareRequests.MapGet("/{id}/files", async (
            Guid id,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            HttpContext http) =>
        {
            var logger = loggerFactory.CreateLogger("ShareRequests.ListFiles");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var (leId, error) = await ValidateAccessAsync(ctx, http, audit);
                if (error is not null) return error;

                var request = await ctx.ShareRequests.FindAsync(id);
                if (request is null || request.LegalEntityId != leId)
                    return Results.NotFound();

                var files = await ctx.ShareRequestFiles
                    .Include(f => f.File)
                    .Where(f => f.ShareRequestId == id)
                    .OrderBy(f => f.File!.CreatedAt)
                    .Select(f => new
                    {
                        f.Id,
                        f.FileId,
                        FileName = f.File!.OriginalName,
                        f.File.SizeBytes,
                        f.File.ContentType,
                        f.File.CreatedAt
                    })
                    .ToListAsync();

                return Results.Ok(files);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка получения файлов требования {Id}", id);
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // DELETE: открепить файл от требования
        shareRequests.MapDelete("/{id}/files/{fileId}", async (
            Guid id,
            Guid fileId,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            HttpContext http) =>
        {
            var logger = loggerFactory.CreateLogger("ShareRequests.DetachFile");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var (leId, error) = await ValidateAccessAsync(ctx, http, audit);
                if (error is not null) return error;

                var request = await ctx.ShareRequests.FindAsync(id);
                if (request is null || request.LegalEntityId != leId)
                    return Results.NotFound();

                var (allowed, statusError) = ValidateStatusForOperation(request, "attach_file");
                if (!allowed)
                    return Results.BadRequest(new { error = statusError });

                var link = await ctx.ShareRequestFiles
                    .FirstOrDefaultAsync(f => f.ShareRequestId == id && f.FileId == fileId);
                if (link is null)
                    return Results.NotFound();

                ctx.ShareRequestFiles.Remove(link);
                await ctx.SaveChangesAsync();

                logger.LogInformation("Файл {FileId} откреплён от требования {RequestId}", fileId, id);

                return Results.Ok();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка открепления файла от требования {Id}", id);
                return Results.BadRequest(new { error = ex.Message });
            }
        });
    }

    private static async Task NotifyCeoAsync(FiduciaDbContext ctx, ShareRequest request, ILogger logger)
    {
        var ceoUsers = await ctx.UserRoles
            .Include(ur => ur.User)
            .Where(ur => ur.Role.Code == "CEO")
            .Select(ur => ur.User!)
            .ToListAsync();

        foreach (var ceo in ceoUsers)
        {
            ctx.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = ceo.Id,
                NotificationType = "COLLECTIVE_DEMAND_THRESHOLD",
                Title = "Коллективное требование набрало порог",
                Body = $"Коллективное требование набрало {request.TotalSupportPercent}% (порог: {request.ThresholdPercent}%). Требуется рассмотрение.",
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    private static async Task NotifySupportersAsync(FiduciaDbContext ctx, ShareRequest request, ILogger logger)
    {
        var supporterIds = await ctx.ShareRequestSupports
            .Where(s => s.ShareRequestId == request.Id && s.WithdrawnAt == null)
            .Select(s => s.ParticipantId)
            .ToListAsync();

        var personIds = await ctx.BoardParticipants
            .Where(p => supporterIds.Contains(p.Id) && p.PersonId != null)
            .Select(p => p.PersonId!.Value)
            .ToListAsync();

        var userIds = await ctx.Users
            .Where(u => personIds.Contains(u.PersonId!.Value))
            .Select(u => u.Id)
            .ToListAsync();

        var decisionText = request.CollectiveStatus == "ACCEPTED" ? "принято" : "отклонено";
        foreach (var userId in userIds)
        {
            ctx.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                NotificationType = "COLLECTIVE_DEMAND_DECISION",
                Title = $"Решение ГД: {decisionText}",
                Body = $"Генеральный директор {decisionText} коллективное требование.",
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    /// <summary>Создание плана ВОСУ из шаблона.</summary>
    private static async Task<Guid?> CreateVosuPlanAsync(
        FiduciaDbContext ctx,
        ITemplateInstantiationService templateService,
        Guid legalEntityId,
        ILogger logger)
    {
        try
        {
            // Инстанцируем шаблон VOSU
            var taskCount = await templateService.InstantiateAsync(
                ctx, "VOSU", legalEntityId, null);

            if (taskCount == 0)
            {
                logger.LogWarning("Шаблон VOSU не найден или нет задач для ЮЛ {LegalEntityId}", legalEntityId);
                return null;
            }

            // Находим созданный OrgIntent (последний для данного ЮЛ с кодом VOSU)
            var orgIntent = await ctx.OrgIntents
                .Include(i => i.TemplateIntent)
                .Where(i => i.LegalEntityId == legalEntityId
                    && i.TemplateIntent!.Code == "VOSU")
                .OrderByDescending(i => i.CreatedAt)
                .FirstOrDefaultAsync();

            if (orgIntent != null)
            {
                logger.LogInformation("Создан план ВОСУ {OrgIntentId} для ЮЛ {LegalEntityId}, задач: {TaskCount}",
                    orgIntent.Id, legalEntityId, taskCount);
            }

            return orgIntent?.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка создания плана ВОСУ для ЮЛ {LegalEntityId}", legalEntityId);
            return null;
        }
    }

    /// <summary>Создание плана ВОСА из шаблона.</summary>
    private static async Task<Guid?> CreateVosaPlanAsync(
        FiduciaDbContext ctx,
        ITemplateInstantiationService templateService,
        Guid legalEntityId,
        ILogger logger)
    {
        try
        {
            // Инстанцируем шаблон VOSA
            var taskCount = await templateService.InstantiateAsync(
                ctx, "VOSA", legalEntityId, null);

            if (taskCount == 0)
            {
                logger.LogWarning("Шаблон VOSA не найден или нет задач для ЮЛ {LegalEntityId}", legalEntityId);
                return null;
            }

            // Находим созданный OrgIntent (последний для данного ЮЛ с кодом VOSA)
            var orgIntent = await ctx.OrgIntents
                .Include(i => i.TemplateIntent)
                .Where(i => i.LegalEntityId == legalEntityId
                    && i.TemplateIntent!.Code == "VOSA")
                .OrderByDescending(i => i.CreatedAt)
                .FirstOrDefaultAsync();

            if (orgIntent != null)
            {
                logger.LogInformation("Создан план ВОСА {OrgIntentId} для ЮЛ {LegalEntityId}, задач: {TaskCount}",
                    orgIntent.Id, legalEntityId, taskCount);
            }

            return orgIntent?.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка создания плана ВОСА для ЮЛ {LegalEntityId}", legalEntityId);
            return null;
        }
    }

    /// <summary>Специфичная валидация по типу запроса.</summary>
    private static async Task<string?> ValidateRequestTypeAsync(
        FiduciaDbContext ctx, RefRequestType requestType, Guid leId, ShareRequestCreateDto dto)
    {
        // Общая проверка: нет ли уже активного запроса того же типа
        var existingPending = await ctx.ShareRequests
            .AnyAsync(r => r.LegalEntityId == leId
                && r.RequestTypeId == requestType.Id
                && (r.Status == "draft" || r.Status == "submitted"));
        if (existingPending)
            return $"Уже есть активное требование типа «{requestType.Name}»";

        return requestType.Code switch
        {
            "NOTARY_LIST_MAINTENANCE" => await ValidateNotaryListMaintenanceAsync(ctx, leId),
            "PREEMPTIVE_LIST" => await ValidatePreemptiveListAsync(ctx, leId),
            "EXIT_APPLICATION" => await ValidateExitApplicationAsync(ctx, leId),
            "CHANGE_STANDARD_CHARTER_NUMBER" => await ValidateChangeStandardCharterNumberAsync(ctx, leId, dto),
            "CONVERT_STANDARD_TO_CUSTOM_CHARTER" => await ValidateConvertToCustomCharterAsync(ctx, leId, dto),
            "CHANGE_CUSTOM_CHARTER_PROVISION" => await ValidateChangeCustomCharterProvisionAsync(ctx, leId),
            "DEMAND_VOSU" => await ValidateDemandVosuAsync(ctx, leId, dto),
            "CONVERT_TO_NJSC" => await ValidateConvertToNjscAsync(ctx, leId),
            "CONVERT_TO_PJSC" => await ValidateConvertToPjscAsync(ctx, leId),
            "CHANGE_CHARTER_PROVISION" => await ValidateChangeCharterProvisionAsync(ctx, leId),
            _ => null
        };
    }

    private static async Task<string?> ValidateNotaryListMaintenanceAsync(FiduciaDbContext ctx, Guid leId)
    {
        var extraSettings = await ctx.LegalEntityExtraSettings
            .FirstOrDefaultAsync(x => x.LegalEntityId == leId);
        if (extraSettings?.NotaryListApproved == true)
            return "Ведение списка участников через нотариат уже утверждено";
        return null;
    }

    private static async Task<string?> ValidatePreemptiveListAsync(FiduciaDbContext ctx, Guid leId)
    {
        var charter = await ctx.LegalEntityCharters.FindAsync(leId);
        if (charter is not null && !charter.PreemptiveRight)
            return "Преимущественное право не действует";
        return null;
    }

    private static async Task<string?> ValidateExitApplicationAsync(FiduciaDbContext ctx, Guid leId)
    {
        var charter = await ctx.LegalEntityCharters.FindAsync(leId);
        if (charter is not null && !charter.ExitAllowed)
            return "Выход из ООО не предусмотрен уставом";
        return null;
    }

    private static async Task<string?> ValidateChangeStandardCharterNumberAsync(FiduciaDbContext ctx, Guid leId, ShareRequestCreateDto dto)
    {
        var le = await ctx.LegalEntities.FirstOrDefaultAsync(x => x.Id == leId);
        if (le?.StandardCharterId is null)
            return "Текущий устав не является типовым";

        if (!string.IsNullOrEmpty(dto.Payload))
        {
            var payload = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(dto.Payload);
            if (payload.TryGetProperty("newCharterNumber", out var newNum))
            {
                var currentCharter = await ctx.RefStandardCharters.FindAsync(le.StandardCharterId);
                if (currentCharter?.Number == newNum.GetString())
                    return "Новый номер типового устава должен отличаться от текущего";
            }
        }

        return null;
    }

    private static async Task<string?> ValidateConvertToCustomCharterAsync(FiduciaDbContext ctx, Guid leId, ShareRequestCreateDto dto)
    {
        var le = await ctx.LegalEntities.FirstOrDefaultAsync(x => x.Id == leId);
        if (le?.StandardCharterId is null)
            return "Текущий устав уже является нетиповым";

        if (string.IsNullOrEmpty(dto.Payload))
            return "Необходимо приложить файл проекта устава";

        return null;
    }

    private static async Task<string?> ValidateConvertToNjscAsync(FiduciaDbContext ctx, Guid leId)
    {
        var le = await ctx.LegalEntities
            .Include(x => x.RefOkopf)
            .FirstOrDefaultAsync(x => x.Id == leId);
        if (le?.RefOkopf?.Code is null)
            return "Не определён тип организации";
        if (!OkopfTypeMapper.IsLlc(le.RefOkopf.Code))
            return "Преобразование в НАО доступно только для ООО";
        return null;
    }

    private static async Task<string?> ValidateConvertToPjscAsync(FiduciaDbContext ctx, Guid leId)
    {
        var le = await ctx.LegalEntities
            .Include(x => x.RefOkopf)
            .FirstOrDefaultAsync(x => x.Id == leId);
        if (le?.RefOkopf?.Code is null)
            return "Не определён тип организации";
        if (le.RefOkopf.Code != OkopfTypeMapper.NjscCode)
            return "Преобразование в ПАО доступно только для НАО";
        return null;
    }

    private static async Task<string?> ValidateChangeCharterProvisionAsync(FiduciaDbContext ctx, Guid leId)
    {
        var le = await ctx.LegalEntities.FirstOrDefaultAsync(x => x.Id == leId);
        if (le?.StandardCharterId is not null)
            return "Устав является типовым; используйте требование «Изменить номер типового устава» или «Изменить типовой устав на нетиповой»";
        return null;
    }

    private static async Task<string?> ValidateChangeCustomCharterProvisionAsync(FiduciaDbContext ctx, Guid leId)
    {
        var le = await ctx.LegalEntities.FirstOrDefaultAsync(x => x.Id == leId);
        if (le?.StandardCharterId is not null)
            return "Устав является типовым; используйте требование «Изменить номер типового устава»";

        return null;
    }

    private static async Task<string?> ValidateDemandVosuAsync(FiduciaDbContext ctx, Guid leId, ShareRequestCreateDto dto)
    {
        // Загружаем порог устава
        var charter = await ctx.LegalEntityCharters.FindAsync(leId);
        var threshold = charter?.VosuThresholdPercent ?? 10m; // по умолчанию 10%

        // Загружаем участника
        var workplace = await ctx.CurrentWorkplaces.FirstOrDefaultAsync();
        if (workplace?.LastSelectedLegalEntityId is null)
            return "Юридическое лицо не выбрано";

        // Находим участника по userId из контекста (передаём через payload)
        decimal? sharePercent = null;
        if (!string.IsNullOrEmpty(dto.Payload))
        {
            var payload = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(dto.Payload);
            if (payload.TryGetProperty("sharePercent", out var sp))
                sharePercent = sp.GetDecimal();
        }

        if (sharePercent is null)
            return "Не указана доля участника";

        if (sharePercent < threshold)
            return $"Доля участника ({sharePercent}%) ниже порога устава ({threshold}%)";

        return null;
    }

    private static async Task<(Guid? leId, IResult? error)> ValidateAccessAsync(
        FiduciaDbContext ctx, HttpContext http, ISecurityAuditService audit)
    {
        var workplace = await ctx.CurrentWorkplaces.FirstOrDefaultAsync();
        var leId = workplace?.LastSelectedLegalEntityId;
        if (leId is null || leId == Guid.Empty)
        {
            await audit.LogEventAsync(AuditActionAccess, "unknown", "Юридическое лицо не выбрано");
            return (null, Results.BadRequest(new { error = "Юридическое лицо не выбрано" }));
        }

        var le = await ctx.LegalEntities
            .Include(x => x.RefOkopf)
            .FirstOrDefaultAsync(x => x.Id == leId.Value);
        if (le?.RefOkopf?.Code is null)
        {
            await audit.LogEventAsync(AuditActionAccess, "unknown", $"Доступ запрещён: не определён тип организации ЮЛ «{le?.Name}»");
            return (null, Results.Forbid());
        }

        var okopfCode = le.RefOkopf.Code;
        if (!OkopfTypeMapper.IsLlc(okopfCode)
            && okopfCode != OkopfTypeMapper.NjscCode
            && !OkopfTypeMapper.IsPjsc(okopfCode))
        {
            await audit.LogEventAsync(AuditActionAccess, "unknown", $"Доступ запрещён: ЮЛ «{le.Name}» имеет неподдерживаемый тип ОКОПФ ({okopfCode})");
            return (null, Results.Forbid());
        }

        return (leId, null);
    }

    private static async Task<Guid?> GetLegalEntityIdAsync(FiduciaDbContext ctx)
    {
        var workplace = await ctx.CurrentWorkplaces.FirstOrDefaultAsync();
        return workplace?.LastSelectedLegalEntityId;
    }

    private static object MapToDto(ShareRequest r) => new
    {
        r.Id,
        r.LegalEntityId,
        RequestTypeId = r.RequestTypeId,
        RequestTypeCode = r.RequestType?.Code,
        RequestTypeName = r.RequestType?.Name,
        r.Status,
        r.Payload,
        r.CreatedAt,
        r.CompletedAt,
        r.RevokedAt,
        r.RevokedByNotarized,
        r.VisibleToAll,
        IsEditable = r.Status == "draft"
    };

    private static object MapToCollectiveDto(ShareRequest r) => new
    {
        r.Id,
        r.LegalEntityId,
        RequestTypeId = r.RequestTypeId,
        RequestTypeCode = r.RequestType?.Code,
        RequestTypeName = r.RequestType?.Name,
        InitiatorName = r.Participant?.FullName ?? r.Participant?.CompanyName,
        r.Payload,
        r.Status,
        r.CollectiveStatus,
        r.ThresholdPercent,
        r.TotalSupportPercent,
        r.SupporterCount,
        r.CreatedAt,
        r.SubmittedToCeoAt,
        r.CeoDecisionAt,
        r.CeoComment,
        r.OrgIntentId,
        IsEditable = r.CollectiveStatus == "COLLECTING"
    };

    /// <summary>Код правовой нормы для типа требования.</summary>
    private static string GetLegalBasisCode(string code) => code switch
    {
        "DEMAND_VOSU" => "article-14fz-35",
        "ADD_AGENDA_OSU" => "article-14fz-36",
        "DEMAND_VOSA" => "article-55",
        "ADD_AGENDA_GOSA" => "article-53",
        _ => ""
    };

    /// <summary>Является ли тип коллективным (требует сбора поддержек).</summary>
    private static bool IsCollectiveTypeCode(string code) => code switch
    {
        "DEMAND_VOSU" => true,
        "ADD_AGENDA_OSU" => true,
        "EXCLUDE_PARTICIPANT" => true,
        "DEMAND_VOSA" => true,
        "ADD_AGENDA_GOSA" => true,
        "DEMAND_INFO_AO" => true,
        _ => false
    };

    /// <summary>Валидация допустимости операции по статусу требования.</summary>
    private static (bool Allowed, string? Error) ValidateStatusForOperation(
        ShareRequest request, string operation, Guid? currentUserId = null)
    {
        return operation switch
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
                || (request.IsCollective && (request.CollectiveStatus == "THRESHOLD_REACHED" || request.CollectiveStatus == "SUBMITTED_TO_CEO"))
                ? (true, null)
                : (false, "Требование не может быть рассмотрено в текущем статусе"),
            "revoke" => request.Status == "submitted"
                ? (true, null)
                : (false, "Отзыв доступен только для отправленных требований"),
            _ => (true, null)
        };
    }
}

public record ShareRequestCreateDto(Guid RequestTypeId, string? Payload);
public record ShareRequestUpdateDto(string? Payload);
public record ShareRequestCompleteDto(string? Status, string? Payload);
public record ShareRequestRevokeDto(bool Notarized);
public record ShareRequestCollectiveCreateDto(Guid RequestTypeId, string? Payload, string? DemandText);
public record ShareRequestDecideDto(string Decision, string? Comment);
public record ShareRequestAttachFileDto(Guid FileId);
