using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Infrastructure.Services;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Services;

public class ParticipantWriteServiceTests : IDisposable
{
    private readonly Infrastructure.Persistence.FiduciaDbContext _ctx;
    private readonly ParticipantWriteService _sut;

    public ParticipantWriteServiceTests()
    {
        var (factory, _) = WriteServiceTestBase.CreateMockFactory(out _ctx);
        var logger = Mock.Of<ILogger<ParticipantWriteService>>();
        var audit = Mock.Of<ISecurityAuditService>();
        var sp = Mock.Of<IServiceProvider>();
        _sut = new ParticipantWriteService(factory, logger, audit, sp);
    }

    public void Dispose() => _ctx.Dispose();

    private void Refresh() => _ctx.ChangeTracker.Clear();

    [Fact]
    public async Task CreateAsync_ValidModel_ReturnsNewId()
    {
        var okopfId = WriteServiceTestBase.SeedOkopf(_ctx, "12300");
        var le = WriteServiceTestBase.SeedLegalEntity(_ctx, okopfId);
        var user = WriteServiceTestBase.SeedUser(_ctx);
        _ = WriteServiceTestBase.SeedEcosystemParticipant(_ctx, user.Id, le.Id);

        var model = new ParticipantCreateModel
        {
            LegalEntityId = le.Id,
            ParticipantType = "FL",
            LastName = "Иванов",
            FirstName = "Пётр",
            SharePercent = 25m,
            PaymentInfo = "Оплачено"
        };

        var id = await _sut.CreateAsync(model, user.Id, "127.0.0.1");

        Refresh();
        id.Should().NotBeEmpty();
        var entity = _ctx.BoardParticipants.FirstOrDefault(p => p.Id == id);
        entity.Should().NotBeNull();
        entity!.LegalEntityId.Should().Be(le.Id);
        entity.ParticipantType.Should().Be("FL");
        entity.IsActive.Should().BeTrue();

        entity.PersonId.Should().NotBeNull();
        var person = _ctx.Persons.FirstOrDefault(p => p.Id == entity.PersonId!.Value);
        person.Should().NotBeNull();
        person!.LastName.Should().Be("Иванов");
        person.FirstName.Should().Be("Пётр");

        var share = _ctx.BoardParticipantShares
            .FirstOrDefault(s => s.ParticipantId == id && s.IsActive);
        share.Should().NotBeNull();
        share!.SharePercent.Should().Be(25m);
    }

    [Fact]
    public async Task CreateAsync_InvalidLlcAccess_ThrowsForNonLlc()
    {
        var okopfId = WriteServiceTestBase.SeedOkopf(_ctx, "12247");
        var le = WriteServiceTestBase.SeedLegalEntity(_ctx, okopfId);
        var user = WriteServiceTestBase.SeedUser(_ctx);
        _ = WriteServiceTestBase.SeedEcosystemParticipant(_ctx, user.Id, le.Id);

        var model = new ParticipantCreateModel
        {
            LegalEntityId = le.Id,
            ParticipantType = "FL",
            LastName = "Иванов",
            FirstName = "Пётр"
        };

        var act = () => _sut.CreateAsync(model, user.Id, "127.0.0.1");
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*не является ООО*");
    }

    [Fact]
    public async Task UpdateAsync_ExistingParticipant_UpdatesFields()
    {
        var okopfId = WriteServiceTestBase.SeedOkopf(_ctx, "12300");
        var le = WriteServiceTestBase.SeedLegalEntity(_ctx, okopfId);
        var user = WriteServiceTestBase.SeedUser(_ctx);
        _ = WriteServiceTestBase.SeedEcosystemParticipant(_ctx, user.Id, le.Id);
        var bp = WriteServiceTestBase.SeedBoardParticipant(_ctx, le.Id);
        WriteServiceTestBase.SeedShare(_ctx, bp.Id, le.Id, 10m);

        var model = new ParticipantUpdateModel
        {
            ParticipantType = "FL",
            LastName = "Петров",
            FirstName = "Алексей",
            SharePercent = 30m,
            PaymentInfo = "Оплачено полностью"
        };

        await _sut.UpdateAsync(bp.Id, model, user.Id, "127.0.0.1");

        Refresh();
        var oldShare = _ctx.BoardParticipantShares
            .FirstOrDefault(s => s.ParticipantId == bp.Id && !s.IsActive);
        oldShare.Should().NotBeNull();
        var newShare = _ctx.BoardParticipantShares
            .FirstOrDefault(s => s.ParticipantId == bp.Id && s.IsActive);
        newShare.Should().NotBeNull();
        newShare!.SharePercent.Should().Be(30m);
    }

    [Fact]
    public async Task DeleteAsync_ExistingParticipant_RemovesIt()
    {
        var okopfId = WriteServiceTestBase.SeedOkopf(_ctx, "12300");
        var le = WriteServiceTestBase.SeedLegalEntity(_ctx, okopfId);
        var user = WriteServiceTestBase.SeedUser(_ctx);
        _ = WriteServiceTestBase.SeedEcosystemParticipant(_ctx, user.Id, le.Id);
        var bp = WriteServiceTestBase.SeedBoardParticipant(_ctx, le.Id);

        await _sut.DeleteAsync(bp.Id, user.Id, "127.0.0.1");

        Refresh();
        var deleted = _ctx.BoardParticipants.FirstOrDefault(p => p.Id == bp.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task CreateTreasuryAsync_ValidModel_ReturnsNewId()
    {
        var okopfId = WriteServiceTestBase.SeedOkopf(_ctx, "12300");
        var le = WriteServiceTestBase.SeedLegalEntity(_ctx, okopfId);
        var user = WriteServiceTestBase.SeedUser(_ctx);
        _ = WriteServiceTestBase.SeedEcosystemParticipant(_ctx, user.Id, le.Id);

        var model = new TreasuryCreateModel
        {
            LegalEntityId = le.Id,
            SharePercent = 3m,
            AcquiredDate = new DateOnly(2024, 6, 1),
            AcquisitionBasis = "Выкуп"
        };

        var id = await _sut.CreateTreasuryAsync(model, user.Id, "127.0.0.1");

        Refresh();
        id.Should().NotBeEmpty();
        var ts = _ctx.BoardTreasuryShares.FirstOrDefault(t => t.Id == id);
        ts.Should().NotBeNull();
        ts!.SharePercent.Should().Be(3m);
        ts.AcquiredDate.Should().Be(new DateOnly(2024, 6, 1));
        ts.SortOrder.Should().Be(1);
    }

    [Fact]
    public async Task DeleteTreasuryAsync_ExistingShare_DeletesIt()
    {
        var okopfId = WriteServiceTestBase.SeedOkopf(_ctx, "12300");
        var le = WriteServiceTestBase.SeedLegalEntity(_ctx, okopfId);
        var user = WriteServiceTestBase.SeedUser(_ctx);
        _ = WriteServiceTestBase.SeedEcosystemParticipant(_ctx, user.Id, le.Id);
        var ts = WriteServiceTestBase.SeedTreasuryShare(_ctx, le.Id, 5m);

        await _sut.DeleteTreasuryAsync(ts.Id, user.Id, "127.0.0.1");

        Refresh();
        var deleted = _ctx.BoardTreasuryShares.FirstOrDefault(t => t.Id == ts.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task CreateChangeAsync_ValidModel_ReturnsNewId()
    {
        var okopfId = WriteServiceTestBase.SeedOkopf(_ctx, "12300");
        var le = WriteServiceTestBase.SeedLegalEntity(_ctx, okopfId);
        var user = WriteServiceTestBase.SeedUser(_ctx);
        var participantRole = WriteServiceTestBase.SeedRole(_ctx, "PARTICIPANT");
        WriteServiceTestBase.SeedUserRole(_ctx, user.Id, participantRole.Id);
        var bp = WriteServiceTestBase.SeedBoardParticipant(_ctx, le.Id);

        var model = new ChangeCreateModel
        {
            LegalEntityId = le.Id,
            ParticipantId = bp.Id,
            ParticipantType = "FL",
            LastName = "Новиков",
            FirstName = "Дмитрий",
            Source = "paper"
        };

        var id = await _sut.CreateChangeAsync(model, user.Id, "127.0.0.1");

        Refresh();
        id.Should().NotBeEmpty();
        var change = _ctx.BoardParticipantChanges.FirstOrDefault(c => c.Id == id);
        change.Should().NotBeNull();
        change!.LastName.Should().Be("Новиков");
        change.Status.Should().Be("pending");
    }

    [Fact]
    public async Task CreateChangeAsync_MissingLastName_Throws()
    {
        var okopfId = WriteServiceTestBase.SeedOkopf(_ctx, "12300");
        var le = WriteServiceTestBase.SeedLegalEntity(_ctx, okopfId);
        var user = WriteServiceTestBase.SeedUser(_ctx);
        var participantRole = WriteServiceTestBase.SeedRole(_ctx, "PARTICIPANT");
        WriteServiceTestBase.SeedUserRole(_ctx, user.Id, participantRole.Id);

        var model = new ChangeCreateModel
        {
            LegalEntityId = le.Id,
            ParticipantId = Guid.NewGuid(),
            FirstName = "Дмитрий",
            Source = "paper"
        };

        var act = () => _sut.CreateChangeAsync(model, user.Id, "127.0.0.1");
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Фамилия обязательна*");
    }
}
