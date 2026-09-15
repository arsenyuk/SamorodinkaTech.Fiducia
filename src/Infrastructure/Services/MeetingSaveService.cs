using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Реализация IMeetingSaveService: сохранение OsaMeeting + BoardOfDirectors + BoardMembers.
/// </summary>
public class MeetingSaveService : IMeetingSaveService
{
    private readonly IDbContextFactory<FiduciaDbContext> _dbFactory;
    private readonly ILogger<MeetingSaveService> _logger;

    public MeetingSaveService(IDbContextFactory<FiduciaDbContext> dbFactory, ILogger<MeetingSaveService> logger)
    {
        _dbFactory = dbFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<MeetingSaveResult> SaveAsync(
        MeetingSaveModel model,
        CancellationToken cancellationToken = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var meeting = await ctx.OsaMeetings
            .FirstOrDefaultAsync(x => x.Id == model.MeetingId, cancellationToken);
        if (meeting is null)
            return new MeetingSaveResult { Error = "Собрание не найдено" };

        if (model.IsGosa)
        {
            // Проверка уникальности года избрания
            if (model.ElectionYear.HasValue && meeting.ElectionYear != model.ElectionYear)
            {
                var duplicate = await ctx.OsaMeetings.AnyAsync(
                    x => x.Id != model.MeetingId && x.ElectionYear == model.ElectionYear.Value,
                    cancellationToken);
                if (duplicate)
                    return new MeetingSaveResult
                    {
                        Error = $"Состав СД за {model.ElectionYear.Value} год уже существует. Нельзя создать более одного состава в году."
                    };
            }

            meeting.GosaWindowStart = model.GosaStart;
            meeting.GosaWindowEnd = model.GosaEnd;
            meeting.ElectionYear = model.ElectionYear;
        }

        if (!model.IsGosa)
            meeting.Title = string.IsNullOrWhiteSpace(model.Title) ? null : model.Title.Trim();

        meeting.ShareholdersCount = model.BoardMandatory ? model.ShareholdersCount : null;
        meeting.BoardMemberNumber = model.BoardMandatory ? model.BoardMemberNumber : null;
        meeting.ExecutiveDirectorsParticipate = model.ExecutiveDirectorsParticipate;
        meeting.ExecutiveDirectorsCount = model.ExecutiveDirectorsParticipate ? model.ExecutiveDirectorsCount : null;
        meeting.NonExecutiveDirectorsParticipate = model.NonExecutiveDirectorsParticipate;
        meeting.NonExecutiveDirectorsCount = model.NonExecutiveDirectorsParticipate ? model.NonExecutiveDirectorsCount : null;
        meeting.IndependentDirectorsParticipate = model.IndependentDirectorsParticipate;
        meeting.IndependentDirectorsCount = model.IndependentDirectorsParticipate ? model.IndependentDirectorsCount : null;
        meeting.ShareholdersListReceived = model.ShareholdersListReceived;
        meeting.AbsenteeVoting = model.AbsenteeVoting;
        meeting.BallotDeadline = model.AbsenteeVoting && model.BallotDeadline.HasValue
            ? model.BallotDeadline.Value.ToUniversalTime() : null;
        meeting.OsaHeld = model.OsaHeld;
        meeting.ProtocolSigned = model.ProtocolSigned && model.OsaHeld;
        meeting.DeputyChairProvided = model.DeputyChairProvided;
        meeting.SecretaryProvided = model.SecretaryProvided;
        meeting.SecretarySignsProtocols = model.SecretaryProvided && model.SecretarySignsProtocols;
        meeting.TemporaryChairProvided = model.TemporaryChairProvided;
        meeting.TemporaryChairSelection = model.TemporaryChairProvided ? model.TemporaryChairSelection : null;
        meeting.BoardCompositionApproved = model.BoardCompositionApproved;
        meeting.BoardMandatory = model.BoardMandatory;
        meeting.BoardApproved = model.BoardApproved;
        meeting.ProtocolSignedAt = model.ProtocolSigned && model.ProtocolSignedAt.HasValue
            ? model.ProtocolSignedAt.Value.ToUniversalTime() : null;

        // BoardOfDirectors
        var board = await ctx.BoardsOfDirectors
            .FirstOrDefaultAsync(x => x.OsaMeetingId == model.MeetingId, cancellationToken);
        Guid boardId;
        if (board is null)
        {
            var draftStatus = await ctx.BoardOfDirectorsStatuses
                .FirstOrDefaultAsync(s => s.Code == "DRAFT", cancellationToken);
            board = new BoardOfDirectors
            {
                Id = Guid.NewGuid(),
                OsaMeetingId = model.MeetingId,
                ElectionYear = model.ElectionYear,
                StatusId = draftStatus?.Id ?? Guid.Empty
            };
            ctx.BoardsOfDirectors.Add(board);

            _logger.LogDebug(
                "DB_CREATE BoardOfDirectors Id={Id} OsaMeetingId={MeetingId} ElectionYear={Year}",
                board.Id, board.OsaMeetingId, board.ElectionYear);
        }
        else
        {
            board.ElectionYear = model.ElectionYear;
        }
        boardId = board.Id;

        // BoardMembers: удаляем старых, создаём новых
        if (model.BoardMembers.Count > 0)
        {
            var oldMembers = await ctx.BoardMembers
                .Where(x => x.OsaMeetingId == model.MeetingId)
                .ToListAsync(cancellationToken);
            var oldMemberIds = oldMembers.Select(x => x.Id).ToList();
            var oldAppointments = await ctx.BoardMemberAppointments
                .Where(x => oldMemberIds.Contains(x.BoardMemberId))
                .ToListAsync(cancellationToken);
            ctx.BoardMemberAppointments.RemoveRange(oldAppointments);
            ctx.BoardMembers.RemoveRange(oldMembers);

            var roles = await ctx.BoardRoles.ToListAsync(cancellationToken);
            var draftStatusId = (await ctx.BoardMemberAppointmentStatuses
                .FirstOrDefaultAsync(s => s.Code == "DRAFT", cancellationToken))?.Id ?? Guid.Empty;

            foreach (var row in model.BoardMembers)
            {
                var member = new BoardMember
                {
                    Id = Guid.NewGuid(),
                    OsaMeetingId = model.MeetingId,
                    BoardOfDirectorsId = boardId,
                    FullName = row.Name.Trim(),
                    BoardMemberTypeId = row.MemberTypeId,
                    Account = string.IsNullOrWhiteSpace(row.Account) ? null : row.Account.Trim(),
                    Email = string.IsNullOrWhiteSpace(row.Email) ? null : row.Email.Trim(),
                    UserId = row.UserId
                };
                ctx.BoardMembers.Add(member);

                _logger.LogDebug(
                    "DB_CREATE BoardMember Id={Id} OsaMeetingId={MeetingId} FullName={Name} UserId={UserId}",
                    member.Id, member.OsaMeetingId, member.FullName, member.UserId);

                if (row.RoleId.HasValue && row.StartedAt is not null && DateOnly.TryParse(row.StartedAt, out var startedAt))
                {
                    var appointment = new BoardMemberAppointment
                    {
                        Id = Guid.NewGuid(),
                        BoardMemberId = member.Id,
                        RoleId = row.RoleId.Value,
                        RoleCode = roles.FirstOrDefault(r => r.Id == row.RoleId.Value)?.Code ?? "",
                        StartedAt = startedAt,
                        StatusId = draftStatusId
                    };
                    ctx.BoardMemberAppointments.Add(appointment);

                    _logger.LogDebug(
                        "DB_CREATE BoardMemberAppointment Id={Id} BoardMemberId={MemberId} RoleCode={RoleCode} StartedAt={Start}",
                        appointment.Id, appointment.BoardMemberId, appointment.RoleCode, appointment.StartedAt);
                }
            }
        }

        await ctx.SaveChangesAsync(cancellationToken);

        return new MeetingSaveResult { Success = true, BoardOfDirectorsId = boardId };
    }
}