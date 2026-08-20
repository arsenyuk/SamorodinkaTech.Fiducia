using Microsoft.EntityFrameworkCore;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.BoardPortal;

/// <summary>
/// Minimal API endpoints для управления повесткой ОСУ (Board Portal).
/// </summary>
public static class AgendaItemEndpoints
{
    public static void MapAgendaItemEndpoints(this WebApplication app)
    {
        var agendaItems = app.MapGroup("/api/agenda-items")
            .RequireAuthorization()
            .WithTags("Agenda Items");

        // ── Legal Entity Extra Settings API ──────────────────────────────
        var extraSettings = app.MapGroup("/api/legal-entity")
            .RequireAuthorization()
            .WithTags("Legal Entity Settings");

        // GET: доп. настройки текущего ЮЛ
        extraSettings.MapGet("/extra-settings", async (
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ILoggerFactory loggerFactory,
            HttpContext http) =>
        {
            var logger = loggerFactory.CreateLogger("LegalEntity.ExtraSettings");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var leId = await GetLegalEntityIdAsync(ctx, http);
                if (leId is null)
                    return Results.Ok(new ExtraSettingsDto(false, null));

                var settings = await ctx.LegalEntityExtraSettings
                    .FirstOrDefaultAsync(x => x.LegalEntityId == leId.Value);

                return Results.Ok(new ExtraSettingsDto(
                    settings?.NotaryListApproved ?? false,
                    settings?.NotaryListDecisionDate?.ToString("dd.MM.yyyy")));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка получения доп. настроек ЮЛ");
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // GET: список пунктов повестки ОСУ для текущего ЮЛ
        agendaItems.MapGet("/", async (
            string? targetType,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ILoggerFactory loggerFactory,
            HttpContext http) =>
        {
            var logger = loggerFactory.CreateLogger("AgendaItems.List");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var leId = await GetLegalEntityIdAsync(ctx, http);
                if (leId is null)
                    return Results.BadRequest(new { error = "Юридическое лицо не выбрано" });

                var query = ctx.AgendaItems
                    .Where(x => x.LegalEntityId == leId.Value);

                if (!string.IsNullOrEmpty(targetType))
                    query = query.Where(x => x.TargetType == targetType);

                var items = await query
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(x => new AgendaItemDto(
                        x.Id, x.Title, x.TargetType, x.Reason, x.Status,
                        x.ShareRequestId, x.CreatedAt))
                    .ToListAsync();

                return Results.Ok(items);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка получения повестки");
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // POST: создать пункт повестки
        agendaItems.MapPost("/", async (
            AgendaItemCreateDto dto,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ILoggerFactory loggerFactory,
            HttpContext http) =>
        {
            var logger = loggerFactory.CreateLogger("AgendaItems.Create");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var leId = await GetLegalEntityIdAsync(ctx, http);
                if (leId is null)
                    return Results.BadRequest(new { error = "Юридическое лицо не выбрано" });

                // Если тип NOTARY_LIST_MAINTENANCE — проверить, не утверждено ли уже
                if (dto.TargetType == "OSA" && dto.Title.Contains("нотариат", StringComparison.OrdinalIgnoreCase))
                {
                    var extraSettings = await ctx.LegalEntityExtraSettings
                        .FirstOrDefaultAsync(x => x.LegalEntityId == leId.Value);
                    if (extraSettings?.NotaryListApproved == true)
                        return Results.BadRequest(new { error = "Ведение списка участников через нотариат уже утверждено" });
                }

                // Найти активный board_of_directors для текущего ЮЛ
                var board = await ctx.BoardsOfDirectors
                    .Where(b => b.OsaMeeting != null && b.OsaMeeting.LegalEntityId == leId.Value)
                    .OrderByDescending(b => b.ElectionYear)
                    .FirstOrDefaultAsync();

                if (board is null)
                    return Results.BadRequest(new { error = "Не найден состав Совета директоров" });

                var item = new AgendaItem
                {
                    Id = Guid.NewGuid(),
                    BoardOfDirectorsId = board.Id,
                    LegalEntityId = leId.Value,
                    ShareRequestId = dto.ShareRequestId,
                    Title = dto.Title,
                    TargetType = dto.TargetType,
                    Reason = dto.Reason,
                    Status = "PENDING"
                };

                ctx.AgendaItems.Add(item);
                await ctx.SaveChangesAsync();

                return Results.Ok(new AgendaItemDto(
                    item.Id, item.Title, item.TargetType, item.Reason, item.Status,
                    item.ShareRequestId, item.CreatedAt));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка создания пункта повестки");
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // POST: принять пункт повестки
        agendaItems.MapPost("/{id:guid}/accept", async (
            Guid id,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ILoggerFactory loggerFactory,
            HttpContext http) =>
        {
            var logger = loggerFactory.CreateLogger("AgendaItems.Accept");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var leId = await GetLegalEntityIdAsync(ctx, http);
                if (leId is null)
                    return Results.BadRequest(new { error = "Юридическое лицо не выбрано" });

                var item = await ctx.AgendaItems
                    .FirstOrDefaultAsync(x => x.Id == id && x.LegalEntityId == leId.Value);

                if (item is null)
                    return Results.NotFound();

                if (item.Status != "PENDING")
                    return Results.BadRequest(new { error = $"Невозможно принять: статус «{item.Status}»" });

                item.Status = "ACCEPTED";
                await ctx.SaveChangesAsync();

                return Results.Ok(new { item.Id, item.Status });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка принятия пункта повестки {Id}", id);
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // POST: отклонить пункт повестки
        agendaItems.MapPost("/{id:guid}/reject", async (
            Guid id,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ILoggerFactory loggerFactory,
            HttpContext http) =>
        {
            var logger = loggerFactory.CreateLogger("AgendaItems.Reject");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var leId = await GetLegalEntityIdAsync(ctx, http);
                if (leId is null)
                    return Results.BadRequest(new { error = "Юридическое лицо не выбрано" });

                var item = await ctx.AgendaItems
                    .FirstOrDefaultAsync(x => x.Id == id && x.LegalEntityId == leId.Value);

                if (item is null)
                    return Results.NotFound();

                if (item.Status != "PENDING")
                    return Results.BadRequest(new { error = $"Невозможно отклонить: статус «{item.Status}»" });

                item.Status = "REJECTED";

                // Если связан с запросом — обновить статус запроса
                if (item.ShareRequestId.HasValue)
                {
                    var shareRequest = await ctx.ShareRequests
                        .FirstOrDefaultAsync(x => x.Id == item.ShareRequestId.Value);
                    if (shareRequest is not null)
                    {
                        shareRequest.Status = "rejected";
                        shareRequest.CompletedAt = DateTime.UtcNow;
                    }
                }

                await ctx.SaveChangesAsync();

                return Results.Ok(new { item.Id, item.Status });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка отклонения пункта повестки {Id}", id);
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // POST: утвердить ведение списка через нотариат (из пункта повестки)
        agendaItems.MapPost("/{id:guid}/approve-notary-list", async (
            Guid id,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ILoggerFactory loggerFactory,
            HttpContext http) =>
        {
            var logger = loggerFactory.CreateLogger("AgendaItems.ApproveNotaryList");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var leId = await GetLegalEntityIdAsync(ctx, http);
                if (leId is null)
                    return Results.BadRequest(new { error = "Юридическое лицо не выбрано" });

                var item = await ctx.AgendaItems
                    .FirstOrDefaultAsync(x => x.Id == id && x.LegalEntityId == leId.Value);

                if (item is null)
                    return Results.NotFound();

                if (item.Status != "PENDING")
                    return Results.BadRequest(new { error = $"Невозможно утвердить: статус «{item.Status}»" });

                item.Status = "ACCEPTED";

                // Обновить extra_settings
                var extraSettings = await ctx.LegalEntityExtraSettings
                    .FirstOrDefaultAsync(x => x.LegalEntityId == leId.Value);
                if (extraSettings is null)
                {
                    extraSettings = new LegalEntityExtraSettings
                    {
                        LegalEntityId = leId.Value,
                        NotaryListApproved = true,
                        NotaryListDecisionDate = DateOnly.FromDateTime(DateTime.UtcNow)
                    };
                    ctx.LegalEntityExtraSettings.Add(extraSettings);
                }
                else
                {
                    extraSettings.NotaryListApproved = true;
                    extraSettings.NotaryListDecisionDate = DateOnly.FromDateTime(DateTime.UtcNow);
                }

                // Обновить статус связанного запроса
                if (item.ShareRequestId.HasValue)
                {
                    var shareRequest = await ctx.ShareRequests
                        .FirstOrDefaultAsync(x => x.Id == item.ShareRequestId.Value);
                    if (shareRequest is not null)
                    {
                        shareRequest.Status = "completed";
                        shareRequest.CompletedAt = DateTime.UtcNow;
                    }
                }

                await ctx.SaveChangesAsync();

                return Results.Ok(new { item.Id, item.Status, NotaryListApproved = true });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка утверждения ведения списка через нотариат {Id}", id);
                return Results.BadRequest(new { error = ex.Message });
            }
        });
    }

    private static async Task<Guid?> GetLegalEntityIdAsync(FiduciaDbContext ctx, HttpContext http)
    {
        var workplace = await ctx.CurrentWorkplaces.FirstOrDefaultAsync();
        return workplace?.LastSelectedLegalEntityId;
    }

    public record AgendaItemDto(Guid Id, string Title, string TargetType, string Reason, string Status, Guid? ShareRequestId, DateTime CreatedAt);
    public record AgendaItemCreateDto(string Title, string TargetType, string Reason, Guid? ShareRequestId);
    public record ExtraSettingsDto(bool NotaryListApproved, string? NotaryListDecisionDate);
}
