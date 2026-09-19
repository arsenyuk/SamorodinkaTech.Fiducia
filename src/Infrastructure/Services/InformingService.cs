using Microsoft.EntityFrameworkCore;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Реализация <see cref="IInformingService"/>.
/// </summary>
public class InformingService : IInformingService
{
    private readonly IDbContextFactory<FiduciaDbContext> _dbFactory;

    public InformingService(IDbContextFactory<FiduciaDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    /// <inheritdoc />
    public async Task<InformingParticipantDto?> GetCurrentParticipantAsync(
        Guid userId, Guid legalEntityId, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(ct);

        var ecoParticipant = await ctx.EcosystemParticipants
            .FirstOrDefaultAsync(ep => ep.UserId == userId, ct);

        if (ecoParticipant is null) return null;

        var participant = await ctx.BoardParticipants
            .Include(bp => bp.Companies.Where(c => c.IsActive).Take(1))
            .Include(bp => bp.Shares.Where(s => s.IsActive).Take(1))
            .FirstOrDefaultAsync(
                bp => bp.LegalEntityId == legalEntityId
                   && bp.EcosystemParticipantId == ecoParticipant.Id, ct);

        if (participant is null) return null;

        Person? person = null;
        if (participant.PersonId.HasValue)
            person = await ctx.Persons.FindAsync(new object[] { participant.PersonId.Value }, ct);

        IdentityDocument? doc = null;
        if (person is not null)
            doc = await ctx.IdentityDocuments
                .FirstOrDefaultAsync(d => d.PersonId == person.Id && d.IsActive, ct);

        var activeCompany = participant.Companies.FirstOrDefault();
        var activeShare = participant.Shares.FirstOrDefault();

        return new InformingParticipantDto(
            participant.Id,
            participant.ParticipantType,
            person?.FullName,
            person?.LastName,
            person?.FirstName,
            person?.MiddleName,
            doc?.DulTypeId,
            doc?.Series,
            doc?.Number,
            doc?.IssuedBy,
            doc?.DepartmentCode,
            doc?.RegistrationAddress,
            person?.Inn,
            person?.Snils,
            person?.Citizenship,
            activeCompany?.CompanyName ?? participant.CompanyName,
            activeCompany?.CompanyInn ?? participant.CompanyInn,
            activeCompany?.CompanyOgrn ?? participant.CompanyOgrn,
            activeCompany?.CompanyKpp ?? participant.CompanyKpp,
            activeCompany?.CompanyAddress ?? participant.CompanyAddress,
            person?.Ogrnip,
            activeShare?.SharePercent,
            activeShare?.ShareAmount);
    }

    /// <inheritdoc />
    public async Task<List<ChangeHistoryItem>> GetChangeHistoryAsync(
        Guid participantId, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(ct);

        var changes = await ctx.BoardParticipantChanges
            .Where(c => c.ParticipantId == participantId)
            .OrderByDescending(c => c.SubmittedAt)
            .ToListAsync(ct);

        var reviewerIds = changes
            .Where(c => c.ReviewedBy.HasValue)
            .Select(c => c.ReviewedBy!.Value)
            .Distinct().ToList();

        var reviewers = reviewerIds.Count > 0
            ? await ctx.Users.Where(u => reviewerIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Login, ct)
            : new Dictionary<Guid, string>();

        return changes.Select(c => new ChangeHistoryItem(
            c.Id,
            c.SubmittedAt,
            c.Status,
            c.ReviewComment,
            c.ReviewedBy.HasValue && reviewers.TryGetValue(c.ReviewedBy.Value, out var login) ? login : null,
            FormatChangeDescription(c)
        )).ToList();
    }

    private static string FormatChangeDescription(BoardParticipantChange ch)
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(ch.LastName))
            parts.Add($"ФИО: {ch.LastName} {ch.FirstName} {ch.MiddleName}");

        if (!string.IsNullOrEmpty(ch.PassportSeries) || !string.IsNullOrEmpty(ch.PassportNumber))
            parts.Add($"Паспорт: {ch.PassportSeries} {ch.PassportNumber}");

        if (ch.SharePercent.HasValue)
            parts.Add($"Доля: {ch.SharePercent.Value:N2}%");

        if (ch.ShareAmount.HasValue)
            parts.Add($"Номинал: {ch.ShareAmount.Value:N2} ₽");

        if (!string.IsNullOrEmpty(ch.CompanyName))
            parts.Add($"ЮЛ: {ch.CompanyName}");

        return parts.Count > 0 ? string.Join(", ", parts) : "—";
    }
}
