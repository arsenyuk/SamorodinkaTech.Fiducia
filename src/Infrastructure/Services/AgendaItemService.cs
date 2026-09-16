using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Infrastructure.Common.Exceptions;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Реализация IAgendaItemService: принятие и отклонение пунктов повестки ОСУ.
/// Логика извлечена из AgendaItemEndpoints (POST accept/reject).
/// </summary>
public class AgendaItemService : IAgendaItemService
{
    private readonly IDbContextFactory<FiduciaDbContext> _dbFactory;
    private readonly ILogger<AgendaItemService> _logger;

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="AgendaItemService"/>.
    /// </summary>
    public AgendaItemService(IDbContextFactory<FiduciaDbContext> dbFactory, ILogger<AgendaItemService> logger)
    {
        _dbFactory = dbFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task AcceptAsync(Guid id, Guid userId, Guid legalEntityId, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(ct);

        var item = await ctx.AgendaItems
            .FirstOrDefaultAsync(x => x.Id == id && x.LegalEntityId == legalEntityId, ct);

        if (item is null)
            throw new InvalidOperationException($"Пункт повестки {id} не найден");

        if (item.Status != "PENDING")
        {
            _logger.LogWarning("Принятие пункта повестки {Id}: невозможно принять, статус «{Status}»", id, item.Status);
            throw new InvalidOperationException($"Невозможно принять: статус «{item.Status}»");
        }

        item.Status = "ACCEPTED";
        await ctx.SaveChangesAsync(ct);

        _logger.LogInformation("Пункт повестки {Id} принят пользователем {UserId}", id, userId);
    }

    /// <inheritdoc />
    public async Task RejectAsync(Guid id, Guid userId, Guid legalEntityId, string? reason, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(ct);

        var item = await ctx.AgendaItems
            .FirstOrDefaultAsync(x => x.Id == id && x.LegalEntityId == legalEntityId, ct);

        if (item is null)
            throw new InvalidOperationException($"Пункт повестки {id} не найден");

        if (item.Status != "PENDING")
        {
            _logger.LogWarning("Отклонение пункта повестки {Id}: невозможно отклонить, статус «{Status}»", id, item.Status);
            throw new InvalidOperationException($"Невозможно отклонить: статус «{item.Status}»");
        }

        item.Status = "REJECTED";

        // Если связан с запросом — обновить статус запроса
        if (item.ShareRequestId.HasValue)
        {
            var shareRequest = await ctx.ShareRequests
                .FirstOrDefaultAsync(x => x.Id == item.ShareRequestId.Value, ct);
            if (shareRequest is not null)
            {
                shareRequest.Status = "rejected";
                shareRequest.CompletedAt = DateTime.UtcNow;
            }
        }

        await ctx.SaveChangesAsync(ct);

        _logger.LogInformation("Пункт повестки {Id} отклонён пользователем {UserId}, причина: {Reason}",
            id, userId, reason ?? "(не указана)");
    }
}
