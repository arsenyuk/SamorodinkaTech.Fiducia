using Microsoft.EntityFrameworkCore;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Реализация <see cref="IParticipantReadService"/>.
/// Заменяет loopback HTTP GET-вызовы из Blazor-страниц к /api/participants.
/// </summary>
public class ParticipantReadService : IParticipantReadService
{
    private readonly IDbContextFactory<FiduciaDbContext> _dbFactory;

    public ParticipantReadService(IDbContextFactory<FiduciaDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    /// <inheritdoc />
    public async Task<List<BoardParticipant>> GetByLegalEntityAsync(Guid legalEntityId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        return await ctx.BoardParticipants
            .Include(p => p.EcosystemParticipant)
            .Include(p => p.Person).ThenInclude(person => person.IdentityDocuments)
            .Include(p => p.Companies.Where(c => c.IsActive).OrderByDescending(c => c.CreatedAt).Take(1))
            .Include(p => p.Shares.Where(s => s.IsActive).OrderByDescending(s => s.CreatedAt).Take(1))
            .Where(p => p.LegalEntityId == legalEntityId)
            .OrderByDescending(p => p.Shares.Where(s => s.IsActive).Select(s => s.SharePercent).FirstOrDefault())
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<BoardParticipant?> GetDetailAsync(Guid id)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        return await ctx.BoardParticipants
            .Include(x => x.EcosystemParticipant).ThenInclude(x => x!.User)
            .Include(x => x.Person)
            .Include(x => x.Companies.Where(c => c.IsActive).OrderByDescending(c => c.CreatedAt).Take(1))
            .Include(x => x.Shares.Where(s => s.IsActive).OrderByDescending(s => s.CreatedAt).Take(1))
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    /// <inheritdoc />
    public async Task<(BoardParticipant? participant, decimal? sharePercent)> GetCurrentAsync(
        Guid userId, Guid legalEntityId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var user = await ctx.Users.FindAsync(userId);
        if (user is null) return (null, null);

        var ecoParticipant = await ctx.EcosystemParticipants
            .FirstOrDefaultAsync(p => p.UserId == user.Id);
        if (ecoParticipant is null) return (null, null);

        var participant = await ctx.BoardParticipants
            .FirstOrDefaultAsync(p =>
                p.LegalEntityId == legalEntityId
                && p.EcosystemParticipantId == ecoParticipant.Id
                && p.IsActive);
        if (participant is null) return (null, null);

        var activeShare = await ctx.BoardParticipantShares
            .FirstOrDefaultAsync(s => s.ParticipantId == participant.Id && s.IsActive);

        return (participant, activeShare?.SharePercent);
    }

    /// <inheritdoc />
    public async Task<List<BoardTreasuryShare>> GetTreasuryAsync(Guid legalEntityId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        return await ctx.BoardTreasuryShares
            .Where(t => t.LegalEntityId == legalEntityId)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<BoardRegistryUpload>> GetRegistryUploadsAsync(Guid legalEntityId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        return await ctx.BoardRegistryUploads
            .Where(u => u.LegalEntityId == legalEntityId)
            .OrderByDescending(u => u.UploadedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<BoardParticipantChange>> GetChangesAsync(
        Guid legalEntityId, Guid? participantId = null)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var query = ctx.BoardParticipantChanges
            .Where(c => c.LegalEntityId == legalEntityId);

        if (participantId.HasValue)
            query = query.Where(c => c.ParticipantId == participantId.Value);

        return await query
            .OrderByDescending(c => c.SubmittedAt)
            .ToListAsync();
    }
}
