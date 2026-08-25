using Microsoft.EntityFrameworkCore;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;

namespace SamorodinkaTech.Fiducia.Domain.Helpers;

/// <summary>
/// Вспомогательные методы для работы с участниками экосистемы через User.Id.
/// </summary>
public static class PersonHelper
{
    /// <summary>
    /// Находит участника экосистемы по идентификатору пользователя.
    /// </summary>
    public static async Task<EcosystemParticipant?> FindParticipantByUserIdAsync(
        IApplicationDbContext db,
        Guid userId,
        CancellationToken ct = default)
    {
        return await db.EcosystemParticipants.FirstOrDefaultAsync(p => p.UserId == userId, ct);
    }

    /// <summary>
    /// Находит участника экосистемы по логину пользователя.
    /// </summary>
    public static async Task<EcosystemParticipant?> FindParticipantByLoginAsync(
        IApplicationDbContext db,
        string login,
        CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Login == login, ct);
        if (user is null) return null;

        return await db.EcosystemParticipants.FirstOrDefaultAsync(p => p.UserId == user.Id, ct);
    }

    /// <summary>
    /// Находит участника СД по пользователю и ЮЛ.
    /// </summary>
    public static async Task<BoardParticipant?> FindParticipantByUserAsync(
        IApplicationDbContext db,
        Guid userId,
        Guid legalEntityId,
        CancellationToken ct = default)
    {
        return await db.BoardParticipants
            .FirstOrDefaultAsync(p =>
                p.LegalEntityId == legalEntityId &&
                p.EcosystemParticipant != null && p.EcosystemParticipant.UserId == userId &&
                p.IsActive, ct);
    }
}
