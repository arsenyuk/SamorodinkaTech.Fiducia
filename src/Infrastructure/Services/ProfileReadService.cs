using Microsoft.EntityFrameworkCore;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Реализация <see cref="IProfileReadService"/>.
/// </summary>
public class ProfileReadService : IProfileReadService
{
    private readonly IDbContextFactory<FiduciaDbContext> _dbFactory;

    public ProfileReadService(IDbContextFactory<FiduciaDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    /// <inheritdoc />
    public async Task<ProfileDto?> GetProfileAsync(Guid userId, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(ct);

        var user = await ctx.Users.FindAsync(new object[] { userId }, ct);
        if (user is null) return null;

        var ecoParticipant = await ctx.EcosystemParticipants
            .FirstOrDefaultAsync(ep => ep.UserId == userId, ct);

        BoardParticipant? participant = null;
        LegalEntity? legalEntity = null;
        BoardParticipantShare? share = null;
        Person? person = null;
        var identityDocuments = new List<IdentityDocument>();

        if (ecoParticipant is not null)
        {
            participant = await ctx.BoardParticipants
                .Include(bp => bp.Shares.Where(s => s.IsActive).Take(1))
                .FirstOrDefaultAsync(bp => bp.EcosystemParticipantId == ecoParticipant.Id, ct);

            if (participant is not null)
            {
                legalEntity = await ctx.LegalEntities.FindAsync(new object[] { participant.LegalEntityId }, ct);
                share = participant.Shares.FirstOrDefault();

                if (participant.PersonId.HasValue)
                {
                    person = await ctx.Persons
                        .Include(p => p.IdentityDocuments).ThenInclude(d => d.DulType)
                        .FirstOrDefaultAsync(p => p.Id == participant.PersonId.Value, ct);

                    if (person is not null)
                        identityDocuments = person.IdentityDocuments
                            .OrderByDescending(d => d.IsActive)
                            .ThenByDescending(d => d.CreatedAt)
                            .ToList();
                }
            }
        }

        // Fallback: person через ecosystem_persons
        if (person is null && ecoParticipant is not null && ecoParticipant.EcosystemPersonId != Guid.Empty)
        {
            var ecoPerson = await ctx.EcosystemPersons.FindAsync(new object[] { ecoParticipant.EcosystemPersonId }, ct);
            if (ecoPerson is not null)
            {
                person = await ctx.Persons
                    .Include(p => p.IdentityDocuments).ThenInclude(d => d.DulType)
                    .FirstOrDefaultAsync(p => p.LastName == ecoPerson.LastName
                        && p.FirstName == ecoPerson.FirstName
                        && p.MiddleName == ecoPerson.MiddleName, ct);

                if (person is not null)
                    identityDocuments = person.IdentityDocuments
                        .OrderByDescending(d => d.IsActive)
                        .ThenByDescending(d => d.CreatedAt)
                        .ToList();
            }
        }

        // История информирования
        var changeHistory = new List<ChangeHistoryItem>();
        if (participant is not null)
        {
            var changes = await ctx.BoardParticipantChanges
                .Where(c => c.ParticipantId == participant.Id)
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

            changeHistory = changes.Select(c => new ChangeHistoryItem(
                c.Id,
                c.SubmittedAt,
                c.Status,
                c.ReviewComment,
                c.ReviewedBy.HasValue && reviewers.TryGetValue(c.ReviewedBy.Value, out var login) ? login : null,
                FormatChangeDescription(c, person)
            )).ToList();
        }

        // Роль в СД
        string? boardRoleName = null;
        if (participant is not null)
        {
            var role = await ctx.BoardParticipantRoles
                .Include(r => r.Role)
                .FirstOrDefaultAsync(r => r.ParticipantId == participant.Id, ct);
            boardRoleName = role?.Role?.Name;
        }

        return new ProfileDto(
            user, person, legalEntity, participant, share, boardRoleName,
            identityDocuments, participant?.IsActive == true, changeHistory);
    }

    private static string FormatChangeDescription(BoardParticipantChange ch, Person? currentPerson)
    {
        var parts = new List<string>();

        if (currentPerson is not null)
        {
            if (!string.IsNullOrEmpty(ch.LastName) && ch.LastName != currentPerson.LastName)
                parts.Add($"ФИО: {ch.LastName} {ch.FirstName} {ch.MiddleName}");
            else if (!string.IsNullOrEmpty(ch.FirstName) && ch.FirstName != currentPerson.FirstName)
                parts.Add($"ФИО: {ch.LastName} {ch.FirstName} {ch.MiddleName}");
        }
        else if (!string.IsNullOrEmpty(ch.LastName))
        {
            parts.Add($"ФИО: {ch.LastName} {ch.FirstName} {ch.MiddleName}");
        }

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
