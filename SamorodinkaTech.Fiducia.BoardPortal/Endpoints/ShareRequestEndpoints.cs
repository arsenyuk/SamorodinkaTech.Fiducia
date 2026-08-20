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

                var types = await ctx.RequestTypes
                    .Where(t => (isLlc && t.IsForLlc) || (isNjsc && t.IsForNjsc) || (isPjsc && t.IsForPjsc))
                    .OrderBy(t => t.Name)
                    .Select(t => new { t.Id, t.Code, t.Name, t.RequiresFile })
                    .ToListAsync();

                return Results.Ok(types);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка получения типов требований");
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

                var entity = new ShareRequest
                {
                    Id = Guid.NewGuid(),
                    LegalEntityId = leId.Value,
                    RequestTypeId = dto.RequestTypeId,
                    Status = "pending",
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

                if (item.Status != "pending" || item.RevokedAt.HasValue)
                    return Results.BadRequest(new { error = "Запрос уже отозван или завершён" });

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
    }

    /// <summary>Специфичная валидация по типу запроса.</summary>
    private static async Task<string?> ValidateRequestTypeAsync(
        FiduciaDbContext ctx, RefRequestType requestType, Guid leId, ShareRequestCreateDto dto)
    {
        return requestType.Code switch
        {
            "NOTARY_LIST_MAINTENANCE" => await ValidateNotaryListMaintenanceAsync(ctx, leId),
            "PREEMPTIVE_LIST" => await ValidatePreemptiveListAsync(ctx, leId),
            "EXIT_APPLICATION" => await ValidateExitApplicationAsync(ctx, leId),
            "CHANGE_STANDARD_CHARTER_NUMBER" => await ValidateChangeStandardCharterNumberAsync(ctx, leId, dto),
            "CONVERT_STANDARD_TO_CUSTOM_CHARTER" => await ValidateConvertToCustomCharterAsync(ctx, leId, dto),
            "CHANGE_CUSTOM_CHARTER_PROVISION" => await ValidateChangeCustomCharterProvisionAsync(ctx, leId),
            "DEMAND_VOSU" => await ValidateDemandVosuAsync(ctx, leId, dto),
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
        if (le?.RefOkopf?.Code is not null && !OkopfTypeMapper.IsLlc(le.RefOkopf.Code))
        {
            await audit.LogEventAsync(AuditActionAccess, "unknown", $"Доступ запрещён: ЮЛ «{le.Name}» не является ООО");
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
        r.VisibleToAll
    };
}

public record ShareRequestCreateDto(Guid RequestTypeId, string? Payload);
public record ShareRequestCompleteDto(string? Status, string? Payload);
public record ShareRequestRevokeDto(bool Notarized);
