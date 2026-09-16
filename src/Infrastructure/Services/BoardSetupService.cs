using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Infrastructure.Common;
using SamorodinkaTech.Fiducia.Infrastructure.Common.Exceptions;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Реализация IBoardSetupService: сохранение состава совета директоров.
/// Логика извлечена из BoardSetupEndpoints (POST /save).
/// </summary>
public class BoardSetupService : IBoardSetupService
{
    private readonly IDbContextFactory<FiduciaDbContext> _dbFactory;
    private readonly ILogger<BoardSetupService> _logger;

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="BoardSetupService"/>.
    /// </summary>
    public BoardSetupService(IDbContextFactory<FiduciaDbContext> dbFactory, ILogger<BoardSetupService> logger)
    {
        _dbFactory = dbFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SaveAsync(BoardSetupSaveModel model, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(ct);

        var leId = model.LegalEntityId;
        if (leId == Guid.Empty)
            throw new InvalidOperationException("Юридическое лицо не определено");

        // Удаляем существующие роли участников этого ЮЛ
        var existingRoles = await ctx.BoardParticipantRoles
            .Include(r => r.Participant)
            .Where(r => r.Participant != null && r.Participant.LegalEntityId == leId)
            .ToListAsync(ct);

        ctx.BoardParticipantRoles.RemoveRange(existingRoles);

        // Создаём новые назначения
        foreach (var assignment in model.Assignments)
        {
            var role = await ctx.BoardRoles.FirstOrDefaultAsync(r => r.Code == assignment.RoleCode, ct);
            if (role is null)
            {
                _logger.LogWarning("Неизвестная роль: {RoleCode}, пропуск назначения участнику {ParticipantId}",
                    assignment.RoleCode, assignment.ParticipantId);
                continue;
            }

            var participantRole = new BoardParticipantRole
            {
                Id = Guid.NewGuid(),
                ParticipantId = assignment.ParticipantId,
                RoleId = role.Id,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = model.UserId
            };

            ctx.BoardParticipantRoles.Add(participantRole);

            _logger.LogInformation("Назначена роль {RoleCode} участнику {ParticipantId}, ЮЛ={LeId}",
                assignment.RoleCode, assignment.ParticipantId, leId);
        }

        await ctx.SaveChangesAsync(ct);

        _logger.LogInformation("Состав СД сохранён для ЮЛ={LeId}: {Count} назначений", leId, model.Assignments.Count);
    }
}
