using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.BoardPortal;

/// <summary>
/// Minimal API endpoints для каталога предоставленных документов (Board Portal).
/// Позволяет участнику просматривать все документы, предоставленные по требованиям REQUEST_INFORMATION,
/// сгруппированные по типам.
/// </summary>
public static class DocumentCatalogEndpoints
{
    public static void MapDocumentCatalogEndpoints(this WebApplication app)
    {
        var catalog = app.MapGroup("/api/documents/catalog")
            .RequireAuthorization()
            .WithTags("Document Catalog");

        // GET: каталог документов — все группы с файлами для текущего участника
        catalog.MapGet("/", async (
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("DocumentCatalog.List");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var leId = await GetLegalEntityIdAsync(ctx);
                if (leId is null)
                    return Results.BadRequest(new { error = "Юридическое лицо не выбрано" });

                // Все принятые требования REQUEST_INFORMATION для текущего ЮЛ
                var requests = await ctx.ShareRequests
                    .Include(r => r.RequestType)
                    .Include(r => r.Items)
                        .ThenInclude(i => i.Files)
                            .ThenInclude(f => f.File)
                    .Where(r => r.RequestType!.Code == "REQUEST_INFORMATION"
                             && r.LegalEntityId == leId
                             && r.ParticipantId != null
                             && (r.Status == "accepted" || r.Status == "completed"))
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                // Собираем каталог: groupCode -> list of demands with items/files
                var catalogGroups = new Dictionary<string, CatalogGroupDto>();

                foreach (var request in requests)
                {
                    // Парсим типы документов из payload для отображения
                    var requestedTypeCodes = ParseDocumentTypeCodes(request.Payload);

                    foreach (var item in request.Items)
                    {
                        // Используем Title группы (GroupName) как ключ каталога
                        var groupKey = item.Title ?? "Другие документы";

                        if (!catalogGroups.TryGetValue(groupKey, out var group))
                        {
                            group = new CatalogGroupDto
                            {
                                GroupName = groupKey,
                                TotalFiles = 0,
                                LatestProvisionDate = null,
                                Demands = new List<CatalogDemandDto>()
                            };
                            catalogGroups[groupKey] = group;
                        }

                        var demand = new CatalogDemandDto
                        {
                            ShareRequestId = request.Id,
                            DemandNumber = $"Требование от {request.CreatedAt:dd.MM.yyyy}",
                            CreatedAt = request.CreatedAt,
                            Status = request.Status,
                            Items = new List<CatalogItemDto>()
                        };

                        var catalogItem = new CatalogItemDto
                        {
                            ItemId = item.Id,
                            Title = item.Title,
                            Files = item.Files.Select(f => new CatalogFileDto
                            {
                                FileId = f.FileId,
                                FileName = f.File?.OriginalName ?? "Файл",
                                SizeBytes = f.File?.SizeBytes ?? 0,
                                ContentType = f.File?.ContentType,
                                ProvisionDate = f.CreatedAt
                            }).ToList()
                        };

                        demand.Items.Add(catalogItem);
                        group.Demands.Add(demand);
                        group.TotalFiles += catalogItem.Files.Count;

                        // Обновляем дату предоставления
                        foreach (var file in catalogItem.Files)
                        {
                            if (group.LatestProvisionDate is null || file.ProvisionDate > group.LatestProvisionDate)
                                group.LatestProvisionDate = file.ProvisionDate;
                        }
                    }
                }

                var result = catalogGroups.Values
                    .OrderBy(g => g.GroupName)
                    .ToList();

                return Results.Ok(new { groups = result });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка получения каталога документов");
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // GET: история предоставления по конкретной группе
        catalog.MapGet("/{groupCode}", async (
            string groupCode,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("DocumentCatalog.GroupHistory");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var leId = await GetLegalEntityIdAsync(ctx);
                if (leId is null)
                    return Results.BadRequest(new { error = "Юридическое лицо не выбрано" });

                // Ищем items с заголовком = groupCode (GroupName)
                var items = await ctx.ShareRequestItems
                    .Include(i => i.ShareRequest)
                        .ThenInclude(r => r!.RequestType)
                    .Include(i => i.Files)
                        .ThenInclude(f => f.File)
                    .Where(i => i.ShareRequest!.RequestType!.Code == "REQUEST_INFORMATION"
                             && i.ShareRequest!.LegalEntityId == leId
                             && i.Title == groupCode
                             && (i.ShareRequest!.Status == "accepted" || i.ShareRequest!.Status == "completed"))
                    .OrderBy(i => i.CreatedAt)
                    .ToListAsync();

                var history = new List<GroupHistoryEntryDto>();
                foreach (var item in items)
                {
                    foreach (var file in item.Files)
                    {
                        history.Add(new GroupHistoryEntryDto
                        {
                            ShareRequestId = item.ShareRequestId,
                            DemandNumber = $"Требование от {item.ShareRequest!.CreatedAt:dd.MM.yyyy}",
                            ItemId = item.Id,
                            FileId = file.FileId,
                            FileName = file.File?.OriginalName ?? "Файл",
                            SizeBytes = file.File?.SizeBytes ?? 0,
                            ContentType = file.File?.ContentType,
                            ProvisionDate = file.CreatedAt
                        });
                    }
                }

                return Results.Ok(new
                {
                    groupCode,
                    entries = history.OrderByDescending(e => e.ProvisionDate).ToList()
                });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка получения истории группы {GroupCode}", groupCode);
                return Results.BadRequest(new { error = ex.Message });
            }
        });
    }

    private static async Task<Guid?> GetLegalEntityIdAsync(FiduciaDbContext ctx)
    {
        var workplace = await ctx.CurrentWorkplaces.FirstOrDefaultAsync();
        return workplace?.LastSelectedLegalEntityId;
    }

    private static List<string> ParseDocumentTypeCodes(string? payload)
    {
        var codes = new List<string>();
        if (string.IsNullOrEmpty(payload)) return codes;
        try
        {
            var doc = JsonDocument.Parse(payload);
            if (doc.RootElement.TryGetProperty("documentTypeCodes", out var codesProp))
            {
                foreach (var code in codesProp.EnumerateArray())
                {
                    var codeStr = code.GetString();
                    if (!string.IsNullOrEmpty(codeStr))
                        codes.Add(codeStr);
                }
            }
        }
        catch { /* payload не JSON */ }
        return codes;
    }
}

// ── DTO ─────────────────────────────────────────────────────────

public class CatalogGroupDto
{
    public string GroupName { get; set; } = "";
    public int TotalFiles { get; set; }
    public DateTime? LatestProvisionDate { get; set; }
    public List<CatalogDemandDto> Demands { get; set; } = new();
}

public class CatalogDemandDto
{
    public Guid ShareRequestId { get; set; }
    public string DemandNumber { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = "";
    public List<CatalogItemDto> Items { get; set; } = new();
}

public class CatalogItemDto
{
    public Guid ItemId { get; set; }
    public string? Title { get; set; }
    public List<CatalogFileDto> Files { get; set; } = new();
}

public class CatalogFileDto
{
    public Guid FileId { get; set; }
    public string FileName { get; set; } = "";
    public long SizeBytes { get; set; }
    public string? ContentType { get; set; }
    public DateTime ProvisionDate { get; set; }
}

public class GroupHistoryEntryDto
{
    public Guid ShareRequestId { get; set; }
    public string DemandNumber { get; set; } = "";
    public Guid ItemId { get; set; }
    public Guid FileId { get; set; }
    public string FileName { get; set; } = "";
    public long SizeBytes { get; set; }
    public string? ContentType { get; set; }
    public DateTime ProvisionDate { get; set; }
}
