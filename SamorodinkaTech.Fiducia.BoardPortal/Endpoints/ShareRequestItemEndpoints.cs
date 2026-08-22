using Microsoft.EntityFrameworkCore;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.BoardPortal;

/// <summary>
/// Minimal API endpoints для пунктов структурированного требования (Board Portal).
/// </summary>
public static class ShareRequestItemEndpoints
{
    public static void MapShareRequestItemEndpoints(this WebApplication app)
    {
        var items = app.MapGroup("/api/share-requests/{requestId:guid}/items")
            .RequireAuthorization()
            .WithTags("Share Request Items");

        // GET: все пункты требования с файлами
        items.MapGet("/", async (
            Guid requestId,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("ShareRequestItems.List");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();

                var requestItems = await ctx.ShareRequestItems
                    .Where(i => i.ShareRequestId == requestId)
                    .OrderBy(i => i.SequenceNumber)
                    .Select(i => new
                    {
                        i.Id,
                        i.SequenceNumber,
                        i.Title,
                        i.Description,
                        i.Status,
                        i.RejectionReason,
                        i.CreatedAt,
                        i.UpdatedAt,
                        Files = i.Files.Select(f => new
                        {
                            f.Id,
                            f.FileId,
                            f.File!.OriginalName,
                            f.File.SizeBytes,
                            f.File.ContentType,
                            f.CreatedAt
                        }).ToList()
                    })
                    .ToListAsync();

                return Results.Ok(requestItems);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка получения пунктов требования {RequestId}", requestId);
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // POST: создать пункт
        items.MapPost("/", async (
            Guid requestId,
            CreateItemDto dto,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("ShareRequestItems.Create");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();

                var request = await ctx.ShareRequests.FindAsync(requestId);
                if (request is null)
                    return Results.NotFound(new { error = "Требование не найдено" });

                if (request.Status != "draft")
                    return Results.BadRequest(new { error = "Редактирование доступно только для черновиков" });

                // Определяем максимальный порядковый номер
                var maxSeq = await ctx.ShareRequestItems
                    .Where(i => i.ShareRequestId == requestId)
                    .MaxAsync(i => (int?)i.SequenceNumber) ?? 0;

                var item = new ShareRequestItem
                {
                    Id = Guid.NewGuid(),
                    ShareRequestId = requestId,
                    SequenceNumber = dto.SequenceNumber > 0 ? dto.SequenceNumber : maxSeq + 1,
                    Title = dto.Title,
                    Description = dto.Description,
                    Status = "pending"
                };

                ctx.ShareRequestItems.Add(item);
                await ctx.SaveChangesAsync();

                return Results.Created($"/api/share-requests/{requestId}/items/{item.Id}", new
                {
                    item.Id,
                    item.SequenceNumber,
                    item.Title,
                    item.Description,
                    item.Status
                });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка создания пункта требования {RequestId}", requestId);
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // PUT: обновить пункт
        items.MapPut("/{itemId:guid}", async (
            Guid requestId, Guid itemId,
            UpdateItemDto dto,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("ShareRequestItems.Update");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();

                var item = await ctx.ShareRequestItems
                    .FirstOrDefaultAsync(i => i.Id == itemId && i.ShareRequestId == requestId);

                if (item is null)
                    return Results.NotFound(new { error = "Пункт не найден" });

                var request = await ctx.ShareRequests.FindAsync(requestId);
                if (request?.Status != "draft")
                    return Results.BadRequest(new { error = "Редактирование доступно только для черновиков" });

                if (dto.Title is not null) item.Title = dto.Title;
                if (dto.Description is not null) item.Description = dto.Description;
                if (dto.SequenceNumber.HasValue) item.SequenceNumber = dto.SequenceNumber.Value;
                item.UpdatedAt = DateTime.UtcNow;

                await ctx.SaveChangesAsync();

                return Results.Ok(new
                {
                    item.Id,
                    item.SequenceNumber,
                    item.Title,
                    item.Description,
                    item.Status
                });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка обновления пункта {ItemId}", itemId);
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // DELETE: удалить пункт
        items.MapDelete("/{itemId:guid}", async (
            Guid requestId, Guid itemId,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("ShareRequestItems.Delete");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();

                var item = await ctx.ShareRequestItems
                    .FirstOrDefaultAsync(i => i.Id == itemId && i.ShareRequestId == requestId);

                if (item is null)
                    return Results.NotFound(new { error = "Пункт не найден" });

                var request = await ctx.ShareRequests.FindAsync(requestId);
                if (request?.Status != "draft")
                    return Results.BadRequest(new { error = "Удаление доступно только для черновиков" });

                ctx.ShareRequestItems.Remove(item);
                await ctx.SaveChangesAsync();

                return Results.Ok(new { success = true });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка удаления пункта {ItemId}", itemId);
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // POST: прикрепить файл к пункту
        items.MapPost("/{itemId:guid}/files", async (
            Guid requestId, Guid itemId,
            AttachFileDto dto,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("ShareRequestItems.AttachFile");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();

                var item = await ctx.ShareRequestItems
                    .FirstOrDefaultAsync(i => i.Id == itemId && i.ShareRequestId == requestId);

                if (item is null)
                    return Results.NotFound(new { error = "Пункт не найден" });

                var fileExists = await ctx.Files.AnyAsync(f => f.Id == dto.FileId);
                if (!fileExists)
                    return Results.BadRequest(new { error = "Файл не найден" });

                // Проверяем дубликат
                var alreadyAttached = await ctx.ShareRequestItemFiles
                    .AnyAsync(f => f.ShareRequestItemId == itemId && f.FileId == dto.FileId);
                if (alreadyAttached)
                    return Results.Ok(new { success = true, message = "Файл уже прикреплён" });

                var fileLink = new ShareRequestItemFile
                {
                    Id = Guid.NewGuid(),
                    ShareRequestItemId = itemId,
                    FileId = dto.FileId
                };

                ctx.ShareRequestItemFiles.Add(fileLink);
                await ctx.SaveChangesAsync();

                return Results.Ok(new { success = true });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка прикрепления файла к пункту {ItemId}", itemId);
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // DELETE: открепить файл от пункта
        items.MapDelete("/{itemId:guid}/files/{fileId:guid}", async (
            Guid requestId, Guid itemId, Guid fileId,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("ShareRequestItems.DetachFile");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();

                var link = await ctx.ShareRequestItemFiles
                    .FirstOrDefaultAsync(f => f.ShareRequestItemId == itemId && f.FileId == fileId);

                if (link is null)
                    return Results.NotFound(new { error = "Связь не найдена" });

                ctx.ShareRequestItemFiles.Remove(link);
                await ctx.SaveChangesAsync();

                return Results.Ok(new { success = true });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка открепления файла {FileId} от пункта {ItemId}", fileId, itemId);
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // POST: принять решение по требованию
        var decideGroup = app.MapGroup("/api/share-requests/{requestId:guid}/decide")
            .RequireAuthorization()
            .WithTags("Share Request Items");

        decideGroup.MapPost("/", async (
            Guid requestId,
            DecideRequestDto dto,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ILoggerFactory loggerFactory,
            HttpContext http) =>
        {
            var logger = loggerFactory.CreateLogger("ShareRequestItems.Decide");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();

                var request = await ctx.ShareRequests
                    .Include(r => r.Items)
                    .FirstOrDefaultAsync(r => r.Id == requestId);

                if (request is null)
                    return Results.NotFound(new { error = "Требование не найдено" });

                var userId = GetUserId(http);
                if (userId is null)
                    return Results.Unauthorized();

                // Определяем пункты для решения
                var itemsToDecide = request.Items.ToList();
                if (dto.Items is { Count: > 0 })
                {
                    // Решение по конкретным пунктам
                    foreach (var itemDecision in dto.Items)
                    {
                        var item = itemsToDecide.FirstOrDefault(i => i.Id == itemDecision.ItemId);
                        if (item is null) continue;

                        item.Status = itemDecision.Status;
                        item.RejectionReason = itemDecision.RejectionReason;
                        item.UpdatedAt = DateTime.UtcNow;
                    }
                }
                else
                {
                    // Решение ко всем пунктам
                    foreach (var item in itemsToDecide)
                    {
                        item.Status = dto.Decision;
                        item.UpdatedAt = DateTime.UtcNow;
                    }
                }

                // Вычисляем общий статус требования
                var statuses = itemsToDecide.Select(i => i.Status).ToList();
                request.DecisionStatus = statuses.All(s => s == "approved") ? "APPROVED"
                    : statuses.All(s => s == "rejected") ? "REJECTED"
                    : "PARTIALLY_APPROVED";
                request.DecisionComment = dto.Comment;
                request.DecidedAt = DateTime.UtcNow;
                request.DecidedByUserId = userId;

                await ctx.SaveChangesAsync();

                return Results.Ok(new
                {
                    request.DecisionStatus,
                    request.DecisionComment,
                    request.DecidedAt,
                    Items = itemsToDecide.Select(i => new
                    {
                        i.Id,
                        i.Status,
                        i.RejectionReason
                    })
                });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка принятия решения по требованию {RequestId}", requestId);
                return Results.BadRequest(new { error = ex.Message });
            }
        });
    }

    private static Guid? GetUserId(HttpContext http)
    {
        var claim = http.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return claim is not null ? Guid.Parse(claim.Value) : null;
    }
}

public record CreateItemDto(string Title, string? Description, int SequenceNumber);
public record UpdateItemDto(string? Title, string? Description, int? SequenceNumber);
public record AttachFileDto(Guid FileId);
public record DecideRequestDto(string Decision, string? Comment, List<ItemDecisionDto>? Items);
public record ItemDecisionDto(Guid ItemId, string Status, string? RejectionReason);
