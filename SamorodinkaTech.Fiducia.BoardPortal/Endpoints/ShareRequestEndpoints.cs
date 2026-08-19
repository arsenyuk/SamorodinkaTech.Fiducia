using Microsoft.EntityFrameworkCore;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Infrastructure;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.BoardPortal;

/// <summary>
/// Minimal API endpoints для запросов участника ООО в общество (Board Portal).
/// Типы: PREEMPTIVE_LIST, NOTARIAL_OFFER, EXIT_APPLICATION, MANDATORY_BUYBACK.
/// Доступно только для ООО (ОКОПФ 12300). Попытки доступа логируются в аудит.
/// </summary>
public static class ShareRequestEndpoints
{
    private const string LlcOkopfCode = "12300";
    private const string AuditActionAccess = "SHARE_REQUEST_ACCESS";

    public static void MapShareRequestEndpoints(this WebApplication app)
    {
        var shareRequests = app.MapGroup("/api/share-requests")
            .RequireAuthorization()
            .WithTags("Share Requests");

        // GET: список запросов текущего участника
        shareRequests.MapGet("/", async (
            string? type,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            HttpContext http) =>
        {
            var logger = loggerFactory.CreateLogger("ShareRequests.List");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var (leId, participantId, error) = await ValidateAccessAsync(ctx, http, audit);
                if (error is not null) return error;

                var query = ctx.ShareRequests
                    .Where(r => r.LegalEntityId == leId && r.ParticipantId == participantId);

                if (!string.IsNullOrEmpty(type))
                    query = query.Where(r => r.RequestType == type);

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
                var (leId, participantId, error) = await ValidateAccessAsync(ctx, http, audit);
                if (error is not null) return error;

                var item = await ctx.ShareRequests
                    .FirstOrDefaultAsync(r => r.Id == id && r.LegalEntityId == leId && r.ParticipantId == participantId);

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
                var (leId, participantId, error) = await ValidateAccessAsync(ctx, http, audit);
                if (error is not null) return error;

                var allowedTypes = new[] { "PREEMPTIVE_LIST", "NOTARIAL_OFFER", "EXIT_APPLICATION", "MANDATORY_BUYBACK" };
                if (!allowedTypes.Contains(dto.RequestType))
                    return Results.BadRequest(new { error = $"Недопустимый тип запроса: {dto.RequestType}" });

                var charter = await ctx.LegalEntityCharters.FindAsync(leId);
                if (charter is not null)
                {
                    if (dto.RequestType == "PREEMPTIVE_LIST" && !charter.PreemptiveRight)
                        return Results.BadRequest(new { error = "Преимущественное право не действует" });

                    if (dto.RequestType == "EXIT_APPLICATION" && !charter.ExitAllowed)
                        return Results.BadRequest(new { error = "Выход из ООО не предусмотрен уставом" });
                }

                var userIdStr = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var createdBy = Guid.TryParse(userIdStr, out var uid) ? uid : Guid.Empty;

                var entity = new ShareRequest
                {
                    Id = Guid.NewGuid(),
                    LegalEntityId = leId,
                    ParticipantId = participantId,
                    RequestType = dto.RequestType,
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
                logger.LogWarning(ex, "Ошибка создания запроса типа {Type}", dto.RequestType);
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
                var (leId, _, error) = await ValidateAccessAsync(ctx, http, audit);
                if (error is not null) return error;

                var item = await ctx.ShareRequests
                    .FirstOrDefaultAsync(r => r.Id == id && r.LegalEntityId == leId);

                if (item is null) return Results.NotFound();

                item.Status = dto.Status ?? "completed";
                item.CompletedAt = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(dto.Payload))
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
                var (leId, participantId, error) = await ValidateAccessAsync(ctx, http, audit);
                if (error is not null) return error;

                var item = await ctx.ShareRequests
                    .FirstOrDefaultAsync(r => r.Id == id && r.LegalEntityId == leId && r.ParticipantId == participantId);

                if (item is null) return Results.NotFound();
                if (item.RequestType != "PREEMPTIVE_LIST")
                    return Results.BadRequest(new { error = "Результат доступен только для запроса списка участников" });
                if (item.Status != "completed")
                    return Results.BadRequest(new { error = "Запрос ещё не завершён" });

                var participants = await ctx.BoardParticipants
                    .Where(p => p.LegalEntityId == leId && p.IsActive && p.Id != participantId)
                    .Select(p => new
                    {
                        ParticipantId = p.Id,
                        p.FullName,
                        p.CompanyName,
                        p.ParticipantType,
                        p.SharePercent,
                        p.ShareAmount
                    })
                    .ToListAsync();

                return Results.Ok(participants);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка получения результата запроса {Id}", id);
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // POST: отзыв оферты в пределах 24 часов
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
                var (leId, participantId, error) = await ValidateAccessAsync(ctx, http, audit);
                if (error is not null) return error;

                var item = await ctx.ShareRequests
                    .FirstOrDefaultAsync(r => r.Id == id && r.LegalEntityId == leId && r.ParticipantId == participantId);

                if (item is null) return Results.NotFound(new { error = "Запрос не найден" });
                if (item.RequestType != "NOTARIAL_OFFER")
                    return Results.BadRequest(new { error = "Отзыв доступен только для нотариальных офертов" });
                if (item.Status != "pending")
                    return Results.BadRequest(new { error = $"Нельзя отозвать запрос со статусом {item.Status}" });
                if (item.RevokedAt.HasValue)
                    return Results.BadRequest(new { error = "Запрос уже отозван" });

                var hoursSinceCreated = (DateTime.UtcNow - item.CreatedAt).TotalHours;
                if (hoursSinceCreated >= 24)
                    return Results.BadRequest(new { error = "Срок отзыва истёк (более 24 часов)" });

                item.Status = "revoked";
                item.RevokedAt = DateTime.UtcNow;
                item.RevokedByNotarized = true;

                await ctx.SaveChangesAsync();
                return Results.Ok(MapToDto(item));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка отзыва запроса {Id}", id);
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // GET: входящий список требований (оферты, видимые всем)
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
                var clientIp = ClientIpHelper.GetClientIp(http);

                var workplace = await ctx.CurrentWorkplaces.FirstOrDefaultAsync();
                var leId = workplace?.LastSelectedLegalEntityId;
                if (leId is null || leId == Guid.Empty)
                    return Results.Ok(Array.Empty<object>());

                // Проверка ООО
                var le = await ctx.LegalEntities
                    .Include(x => x.RefOkopf)
                    .FirstOrDefaultAsync(x => x.Id == leId.Value);

                if (le?.RefOkopf?.Code != LlcOkopfCode)
                {
                    await audit.LogEventAsync(AuditActionAccess, clientIp,
                        $"Доступ запрещён: ЮЛ «{le?.Name}» (ОКОПФ {le?.RefOkopf?.Code}) не является ООО",
                        entityName: "LegalEntity", entityId: leId);
                    return Results.Forbid();
                }

                var items = await ctx.ShareRequests
                    .Where(r => r.LegalEntityId == leId.Value
                        && r.RequestType == "NOTARIAL_OFFER"
                        && r.VisibleToAll
                        && r.Status != "revoked")
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                return Results.Ok(items.Select(MapToDto));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка получения входящего списка");
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // GET: текущий устав ООО (для UI)
        app.MapGet("/api/charter/current", async (
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            HttpContext http) =>
        {
            var logger = loggerFactory.CreateLogger("Charter.Current");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var workplace = await ctx.CurrentWorkplaces.FirstOrDefaultAsync();
                var leId = workplace?.LastSelectedLegalEntityId;
                if (leId is null || leId == Guid.Empty)
                    return Results.Ok(new { preemptiveRight = true, exitAllowed = false, transferToThirdPartiesWithoutConsent = false });

                var charter = await ctx.LegalEntityCharters.FindAsync(leId.Value);
                if (charter is null)
                    return Results.Ok(new { preemptiveRight = true, exitAllowed = false, transferToThirdPartiesWithoutConsent = false });

                return Results.Ok(new
                {
                    preemptiveRight = charter.PreemptiveRight,
                    exitAllowed = charter.ExitAllowed,
                    transferToThirdPartiesWithoutConsent = charter.TransferToThirdPartiesWithoutConsent
                });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка получения данных устава");
                return Results.BadRequest(new { error = ex.Message });
            }
        }).RequireAuthorization();
    }

    /// <summary>
    /// Проверка доступа: ЮЛ — ООО, пользователь — PARTICIPANT, участник в реестре.
    /// </summary>
    private static async Task<(Guid leId, Guid participantId, IResult? error)> ValidateAccessAsync(
        FiduciaDbContext ctx,
        HttpContext http,
        ISecurityAuditService audit)
    {
        var clientIp = ClientIpHelper.GetClientIp(http);

        var workplace = await ctx.CurrentWorkplaces.FirstOrDefaultAsync();
        var leId = workplace?.LastSelectedLegalEntityId;
        if (leId is null || leId == Guid.Empty)
            return (Guid.Empty, Guid.Empty, Results.BadRequest(new { error = "Юридическое лицо не выбрано" }));

        var le = await ctx.LegalEntities
            .Include(x => x.RefOkopf)
            .FirstOrDefaultAsync(x => x.Id == leId.Value);

        if (le is null)
            return (Guid.Empty, Guid.Empty, Results.BadRequest(new { error = "Юридическое лицо не найдено" }));

        var (login, fullName) = await GetUserInfoAsync(ctx, http);

        if (le.RefOkopf?.Code != LlcOkopfCode)
        {
            await audit.LogEventAsync(AuditActionAccess, clientIp,
                $"Доступ запрещён: пользователь {login} ({fullName}), ЮЛ «{le.Name}» (ОКОПФ {le.RefOkopf?.Code}) не является ООО",
                entityName: "LegalEntity", entityId: le.Id);
            return (Guid.Empty, Guid.Empty, Results.Forbid());
        }

        var userIdStr = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            await audit.LogEventAsync(AuditActionAccess, clientIp,
                $"Доступ запрещён: пользователь не аутентифицирован",
                entityName: "LegalEntity", entityId: le.Id);
            return (Guid.Empty, Guid.Empty, Results.Forbid());
        }

        var hasParticipantRole = await ctx.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.Role != null && ur.Role.Code == "PARTICIPANT");

        if (!hasParticipantRole)
        {
            await audit.LogEventAsync(AuditActionAccess, clientIp,
                $"Доступ запрещён: пользователь {login} ({fullName}) не имеет роль PARTICIPANT",
                entityName: "LegalEntity", entityId: le.Id);
            return (Guid.Empty, Guid.Empty, Results.Forbid());
        }

        var user = await ctx.Users.FindAsync(userId);
        if (user?.PersonId is null)
        {
            await audit.LogEventAsync(AuditActionAccess, clientIp,
                $"Доступ запрещён: пользователь {login} ({fullName}) не привязан к участнику",
                entityName: "LegalEntity", entityId: le.Id);
            return (Guid.Empty, Guid.Empty, Results.BadRequest(new { error = "Пользователь не привязан к участнику" }));
        }

        var participant = await ctx.BoardParticipants
            .FirstOrDefaultAsync(p => p.LegalEntityId == leId.Value && p.Id == user.PersonId);

        if (participant is null)
        {
            await audit.LogEventAsync(AuditActionAccess, clientIp,
                $"Доступ запрещён: участник не найден в реестре ЮЛ «{le.Name}»",
                entityName: "LegalEntity", entityId: le.Id);
            return (Guid.Empty, Guid.Empty, Results.BadRequest(new { error = "Участник не найден в реестре" }));
        }

        await audit.LogEventAsync(AuditActionAccess, clientIp,
            $"Доступ разрешён: пользователь {login} ({fullName}), роль PARTICIPANT, ЮЛ «{le.Name}» (ООО)",
            entityName: "LegalEntity", entityId: le.Id);

        return (leId.Value, participant.Id, null);
    }

    private static async Task<(string login, string fullName)> GetUserInfoAsync(
        FiduciaDbContext ctx, HttpContext http)
    {
        var userIdStr = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return ("anonymous", "Неизвестный пользователь");

        var user = await ctx.Users.FindAsync(userId);
        if (user is null)
            return ("unknown", "Пользователь не найден");

        var login = user.Email;
        var fullName = string.IsNullOrWhiteSpace(user.MiddleName)
            ? $"{user.LastName} {user.FirstName}"
            : $"{user.LastName} {user.FirstName} {user.MiddleName}";

        return (login, fullName);
    }

    private static object MapToDto(ShareRequest r) => new
    {
        r.Id,
        r.LegalEntityId,
        r.ParticipantId,
        r.RequestType,
        r.Status,
        r.Payload,
        r.CreatedAt,
        r.CompletedAt,
        r.RevokedAt,
        r.RevokedByNotarized,
        r.VisibleToAll
    };
}

public record ShareRequestCreateDto(string RequestType, string? Payload);
public record ShareRequestCompleteDto(string? Status, string? Payload);
public record ShareRequestRevokeDto(bool Notarized);
