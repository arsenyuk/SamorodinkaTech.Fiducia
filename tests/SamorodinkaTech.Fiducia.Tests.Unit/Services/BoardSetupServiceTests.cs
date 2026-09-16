using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Infrastructure.Services;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Services;

public class BoardSetupServiceTests : IDisposable
{
    private readonly Infrastructure.Persistence.FiduciaDbContext _ctx;
    private readonly BoardSetupService _sut;

    public BoardSetupServiceTests()
    {
        var (factory, _) = WriteServiceTestBase.CreateMockFactory(out _ctx);
        var logger = Mock.Of<ILogger<BoardSetupService>>();
        _sut = new BoardSetupService(factory, logger);
    }

    public void Dispose() => _ctx.Dispose();

    private void Refresh() => _ctx.ChangeTracker.Clear();

    [Fact]
    public async Task SaveAsync_ValidModel_SavesToDb()
    {
        var okopfId = WriteServiceTestBase.SeedOkopf(_ctx);
        var le = WriteServiceTestBase.SeedLegalEntity(_ctx, okopfId);
        var boardRole = WriteServiceTestBase.SeedBoardRole(_ctx, "BOARD_CHAIRMAN");
        var participant1 = WriteServiceTestBase.SeedBoardParticipant(_ctx, le.Id);
        var participant2 = WriteServiceTestBase.SeedBoardParticipant(_ctx, le.Id);

        var model = new BoardSetupSaveModel
        {
            LegalEntityId = le.Id,
            UserId = Guid.NewGuid(),
            Assignments = new List<BoardSetupAssignmentModel>
            {
                new() { ParticipantId = participant1.Id, RoleCode = "BOARD_CHAIRMAN" },
                new() { ParticipantId = participant2.Id, RoleCode = "BOARD_CHAIRMAN" }
            }
        };

        await _sut.SaveAsync(model);

        Refresh();
        var roles = _ctx.BoardParticipantRoles
            .Where(r => r.ParticipantId == participant1.Id || r.ParticipantId == participant2.Id)
            .ToList();
        roles.Should().HaveCount(2);
        roles.All(r => r.RoleId == boardRole.Id).Should().BeTrue();
    }

    [Fact]
    public async Task SaveAsync_EmptyLegalEntityId_Throws()
    {
        var model = new BoardSetupSaveModel
        {
            LegalEntityId = Guid.Empty,
            UserId = Guid.NewGuid(),
            Assignments = new List<BoardSetupAssignmentModel>()
        };

        var act = () => _sut.SaveAsync(model);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*не определено*");
    }

    [Fact]
    public async Task SaveAsync_ExistingRoles_ReplacesAll()
    {
        var okopfId = WriteServiceTestBase.SeedOkopf(_ctx);
        var le = WriteServiceTestBase.SeedLegalEntity(_ctx, okopfId);
        var boardRole = WriteServiceTestBase.SeedBoardRole(_ctx, "BOARD_SECRETARY");
        var participant = WriteServiceTestBase.SeedBoardParticipant(_ctx, le.Id);

        var oldRoleId = Guid.NewGuid();
        var oldRole = new BoardParticipantRole
        {
            Id = oldRoleId,
            ParticipantId = participant.Id,
            RoleId = boardRole.Id
        };
        _ctx.BoardParticipantRoles.Add(oldRole);
        _ctx.SaveChanges();

        var model = new BoardSetupSaveModel
        {
            LegalEntityId = le.Id,
            UserId = Guid.NewGuid(),
            Assignments = new List<BoardSetupAssignmentModel>
            {
                new() { ParticipantId = participant.Id, RoleCode = "BOARD_SECRETARY" }
            }
        };

        await _sut.SaveAsync(model);

        Refresh();
        var roles = _ctx.BoardParticipantRoles.Where(r => r.ParticipantId == participant.Id).ToList();
        roles.Should().HaveCount(1);
        roles[0].Id.Should().NotBe(oldRoleId);
    }

    [Fact]
    public async Task SaveAsync_UnknownRoleCode_SkipsAssignment()
    {
        var okopfId = WriteServiceTestBase.SeedOkopf(_ctx);
        var le = WriteServiceTestBase.SeedLegalEntity(_ctx, okopfId);
        var participant = WriteServiceTestBase.SeedBoardParticipant(_ctx, le.Id);

        var model = new BoardSetupSaveModel
        {
            LegalEntityId = le.Id,
            UserId = Guid.NewGuid(),
            Assignments = new List<BoardSetupAssignmentModel>
            {
                new() { ParticipantId = participant.Id, RoleCode = "NON_EXISTENT_ROLE" }
            }
        };

        await _sut.SaveAsync(model);

        Refresh();
        _ctx.BoardParticipantRoles.Count().Should().Be(0);
    }
}
