using Microsoft.EntityFrameworkCore;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Validation;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Реализация IMeetingLoadService: загрузка данных собрания для редактирования.
/// </summary>
public class MeetingLoadService : IMeetingLoadService
{
    private readonly IDbContextFactory<FiduciaDbContext> _dbFactory;

    public MeetingLoadService(IDbContextFactory<FiduciaDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    /// <inheritdoc />
    public async Task<MeetingEditData?> LoadAsync(
        Guid meetingId,
        CancellationToken cancellationToken = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var meeting = await ctx.OsaMeetings
            .FirstOrDefaultAsync(x => x.Id == meetingId, cancellationToken);
        if (meeting is null) return null;

        var le = await ctx.LegalEntities
            .Include(x => x.RefOkopf)
            .FirstOrDefaultAsync(x => x.Id == meeting.LegalEntityId, cancellationToken);

        var board = await ctx.BoardsOfDirectors
            .FirstOrDefaultAsync(x => x.OsaMeetingId == meetingId, cancellationToken);

        var members = await ctx.BoardMembers
            .Where(x => x.OsaMeetingId == meetingId)
            .ToListAsync(cancellationToken);

        var memberIds = members.Select(x => x.Id).ToList();
        var appointments = await ctx.BoardMemberAppointments
            .Where(x => memberIds.Contains(x.BoardMemberId))
            .ToListAsync(cancellationToken);

        var boardMembers = members.Select(m =>
        {
            var appt = appointments.FirstOrDefault(a => a.BoardMemberId == m.Id);
            return new BoardMemberRowModel
            {
                Name = m.FullName,
                MemberTypeId = m.BoardMemberTypeId,
                RoleId = appt?.RoleId,
                Account = m.Account,
                Email = m.Email,
                UserId = m.UserId,
                StartedAt = appt?.StartedAt.ToString("yyyy-MM-dd")
            };
        }).ToList();

        return new MeetingEditData
        {
            Meeting = meeting,
            LegalEntity = le!,
            OkopfCode = le?.RefOkopf?.Code ?? "",
            IsPjsc = OkopfTypeMapper.IsPjsc(le?.RefOkopf?.Code),
            BoardOfDirectors = board,
            BoardMembers = boardMembers
        };
    }
}