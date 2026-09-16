using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Реализация <see cref="IShareRequestReadService"/>.
/// Заменяет loopback HTTP GET-вызовы из Blazor-страниц к /api/share-requests.
/// </summary>
public class ShareRequestReadService : IShareRequestReadService
{
    private readonly IDbContextFactory<FiduciaDbContext> _dbFactory;
    private readonly ILogger<ShareRequestReadService> _logger;

    public ShareRequestReadService(
        IDbContextFactory<FiduciaDbContext> dbFactory,
        ILogger<ShareRequestReadService> logger)
    {
        _dbFactory = dbFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<ShareRequest>> GetListAsync(Guid userId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        // User → EcosystemParticipants (by userId)
        var ecoParticipantIds = await ctx.EcosystemParticipants
            .Where(ep => ep.UserId == userId && ep.IsActive)
            .Select(ep => ep.Id)
            .ToListAsync();

        // EcosystemParticipants → BoardParticipants (by EcosystemParticipantId)
        var boardParticipantIds = await ctx.BoardParticipants
            .Where(bp => ecoParticipantIds.Contains(bp.EcosystemParticipantId!.Value) && bp.IsActive)
            .Select(bp => bp.Id)
            .ToListAsync();

        // BoardParticipants → ShareRequests (participant or creator)
        return await ctx.ShareRequests
            .Include(r => r.RequestType)
            .Where(r => boardParticipantIds.Contains(r.ParticipantId) || r.CreatedBy == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<RefRequestType>> GetTypesAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.RequestTypes.OrderBy(t => t.Name).ToListAsync();
    }

    /// <inheritdoc />
    public async Task<ShareRequestRefData> GetRefDataAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var documentTypes = await ctx.DocumentTypes
            .OrderBy(d => d.SortOrder)
            .ToListAsync();

        var documentGroups = documentTypes
            .GroupBy(d => d.GroupCode)
            .Select(g => new DocumentGroup
            {
                Code = g.Key,
                Name = g.First().GroupName,
                Documents = g.ToList()
            })
            .ToList();

        var accessMethodsRaw = await ctx.DocumentAccessMethods
            .OrderBy(m => m.SortOrder)
            .ToListAsync();

        var accessMethods = accessMethodsRaw.Select(m => new DocumentAccessMethod
        {
            Id = m.Id,
            Code = m.Code,
            Name = m.Name,
            Description = m.Description,
            DeadlineDays = m.DeadlineDays
        }).ToList();

        return new ShareRequestRefData
        {
            DocumentGroups = documentGroups,
            AccessMethods = accessMethods
        };
    }

    /// <inheritdoc />
    public async Task<ShareRequest?> GetDetailAsync(Guid id)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.ShareRequests
            .Include(r => r.RequestType)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    /// <inheritdoc />
    public async Task<List<ShareRequestSupport>> GetSupportsAsync(Guid requestId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.ShareRequestSupports
            .Include(s => s.Participant)
            .ThenInclude(p => p!.Person)
            .Where(s => s.ShareRequestId == requestId)
            .OrderByDescending(s => s.SupportedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<ShareRequestFile>> GetFilesAsync(Guid requestId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.ShareRequestFiles
            .Include(f => f.File)
            .Where(f => f.ShareRequestId == requestId)
            .OrderBy(f => f.File!.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<ShareRequestItem>> GetItemsAsync(Guid requestId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.ShareRequestItems
            .Include(i => i.Files).ThenInclude(f => f.File)
            .Where(i => i.ShareRequestId == requestId)
            .OrderBy(i => i.SequenceNumber)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<ShareRequestParticipantResult>> GetResultAsync(Guid requestId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var request = await ctx.ShareRequests
            .Include(r => r.RequestType)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request?.RequestType?.Code != "PREEMPTIVE_LIST" || request.Status != "completed")
        {
            _logger.LogDebug(
                "GetResultAsync: заявка {RequestId} не является завершённой PREEMPTIVE_LIST (тип={TypeCode}, статус={Status})",
                requestId, request?.RequestType?.Code, request?.Status);
            return new List<ShareRequestParticipantResult>();
        }

        var participants = await ctx.BoardParticipants
            .Where(p => p.LegalEntityId == request.LegalEntityId && p.IsActive)
            .Include(p => p.Person)
            .Include(p => p.Shares.Where(s => s.IsActive).Take(1))
            .ToListAsync();

        return participants.Select(p => new ShareRequestParticipantResult
        {
            FullName = p.Person?.FullName,
            CompanyName = p.CompanyName,
            CompanyInn = p.CompanyInn,
            ParticipantType = p.ParticipantType,
            SharePercent = p.Shares.FirstOrDefault()?.SharePercent,
            ShareAmount = p.Shares.FirstOrDefault()?.ShareAmount
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<List<Notification>> GetNotificationsAsync(Guid requestId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var requestIdStr = requestId.ToString();
        return await ctx.Notifications
            .Where(n => n.Url != null && n.Url.Contains(requestIdStr))
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<CeoDemandDetail?> GetCeoDemandAsync(Guid requestId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var request = await ctx.ShareRequests
            .Include(r => r.RequestType)
            .Include(r => r.Participant)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request is null)
            return null;

        var legalEntity = await ctx.LegalEntities.FindAsync(request.LegalEntityId);

        BoardParticipant? participant = null;
        if (request.ParticipantId != Guid.Empty)
        {
            participant = await ctx.BoardParticipants
                .Include(p => p.Person)
                .FirstOrDefaultAsync(p => p.Id == request.ParticipantId);
        }

        var files = await ctx.ShareRequestFiles
            .Include(f => f.File)
            .Where(f => f.ShareRequestId == requestId)
            .ToListAsync();

        return new CeoDemandDetail
        {
            Request = request,
            LegalEntity = legalEntity!,
            Participant = participant,
            Files = files
        };
    }

    /// <inheritdoc />
    public async Task<List<CeoDemandListItem>> GetCeoReviewListAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        // Требования для ГД: достигшие порога коллективные + все submitted одиночные,
        // исключая требования на рассмотрении СД (BOARD_REVIEW)
        var items = await ctx.ShareRequests
            .Include(r => r.RequestType)
            .Include(r => r.Participant).ThenInclude(p => p!.Person)
            .Include(r => r.LegalEntity)
            .Where(r => r.CollectiveStatus != "BOARD_REVIEW")
            .Where(r =>
                (r.IsCollective && r.CollectiveStatus == "THRESHOLD_REACHED")
                || (!r.IsCollective && r.Status == "submitted"))
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return items.Select(r => new CeoDemandListItem
        {
            Id = r.Id,
            RequestTypeName = r.RequestType?.Name ?? "",
            LegalEntityName = r.LegalEntity?.Name ?? "",
            ParticipantName = r.Participant?.Person?.FullName ?? r.Participant?.CompanyName ?? "",
            Status = r.CollectiveStatus ?? r.Status,
            CreatedAt = r.CreatedAt
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<List<VosuNotification>> GetVosuNotificationsAsync(Guid requestId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var request = await ctx.ShareRequests
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request?.OrgIntentId is null)
            return new List<VosuNotification>();

        return await ctx.VosuNotifications
            .Include(n => n.BoardParticipant)
                .ThenInclude(p => p!.Person)
            .Include(n => n.File)
            .Where(n => n.OrgIntentId == request.OrgIntentId.Value)
            .OrderBy(n => n.BoardParticipant!.Person != null
                ? n.BoardParticipant.Person.LastName
                : n.BoardParticipant.CompanyName)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<DocumentCatalogGroup>> GetDocumentCatalogAsync(Guid legalEntityId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var requests = await ctx.ShareRequests
            .Include(r => r.RequestType)
            .Include(r => r.Items)
                .ThenInclude(i => i.Files)
                    .ThenInclude(f => f.File)
            .Where(r => r.RequestType!.Code == "REQUEST_INFORMATION"
                     && r.LegalEntityId == legalEntityId
                     && (r.Status == "accepted" || r.Status == "completed"))
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var catalogGroups = new Dictionary<string, DocumentCatalogGroup>();

        foreach (var request in requests)
        {
            foreach (var item in request.Items)
            {
                var groupKey = item.Title ?? "Другие документы";

                if (!catalogGroups.TryGetValue(groupKey, out var group))
                {
                    group = new DocumentCatalogGroup
                    {
                        GroupName = groupKey,
                        TotalFiles = 0,
                        LatestProvisionDate = null,
                        Demands = new List<DocumentCatalogDemand>()
                    };
                    catalogGroups[groupKey] = group;
                }

                var demand = new DocumentCatalogDemand
                {
                    ShareRequestId = request.Id,
                    DemandNumber = $"Требование от {request.CreatedAt:dd.MM.yyyy}",
                    CreatedAt = request.CreatedAt,
                    Status = request.Status,
                    Items = new List<DocumentCatalogItem>()
                };

                var catalogItem = new DocumentCatalogItem
                {
                    ItemId = item.Id,
                    Title = item.Title,
                    Files = item.Files.Select(f => new DocumentCatalogFile
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

                foreach (var file in catalogItem.Files)
                {
                    if (group.LatestProvisionDate is null || file.ProvisionDate > group.LatestProvisionDate)
                        group.LatestProvisionDate = file.ProvisionDate;
                }
            }
        }

        return catalogGroups.Values
            .OrderBy(g => g.GroupName)
            .ToList();
    }
}
