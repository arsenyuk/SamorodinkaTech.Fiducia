using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Infrastructure.Services;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Services;

public class AgendaItemServiceTests : IDisposable
{
    private readonly Infrastructure.Persistence.FiduciaDbContext _ctx;
    private readonly AgendaItemService _sut;

    public AgendaItemServiceTests()
    {
        var (factory, _) = WriteServiceTestBase.CreateMockFactory(out _ctx);
        var logger = Mock.Of<ILogger<AgendaItemService>>();
        _sut = new AgendaItemService(factory, logger);
    }

    public void Dispose() => _ctx.Dispose();

    private void Refresh() => _ctx.ChangeTracker.Clear();

    [Fact]
    public async Task AcceptAsync_ExistingItem_SetsStatusAccepted()
    {
        var leId = Guid.NewGuid();
        var item = WriteServiceTestBase.SeedAgendaItem(_ctx, leId, status: "PENDING");

        await _sut.AcceptAsync(item.Id, Guid.NewGuid(), leId);

        Refresh();
        var updated = _ctx.AgendaItems.FirstOrDefault(a => a.Id == item.Id);
        updated.Should().NotBeNull();
        updated!.Status.Should().Be("ACCEPTED");
    }

    [Fact]
    public async Task RejectAsync_ExistingItem_SetsStatusRejected()
    {
        var leId = Guid.NewGuid();
        var item = WriteServiceTestBase.SeedAgendaItem(_ctx, leId, status: "PENDING");

        await _sut.RejectAsync(item.Id, Guid.NewGuid(), leId, "Неактуально");

        Refresh();
        var updated = _ctx.AgendaItems.FirstOrDefault(a => a.Id == item.Id);
        updated.Should().NotBeNull();
        updated!.Status.Should().Be("REJECTED");
    }

    [Fact]
    public async Task AcceptAsync_ItemNotFound_Throws()
    {
        var act = () => _sut.AcceptAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*не найден*");
    }

    [Fact]
    public async Task AcceptAsync_NonPendingStatus_Throws()
    {
        var leId = Guid.NewGuid();
        var item = WriteServiceTestBase.SeedAgendaItem(_ctx, leId, status: "ACCEPTED");

        var act = () => _sut.AcceptAsync(item.Id, Guid.NewGuid(), leId);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Невозможно принять*");
    }

    [Fact]
    public async Task RejectAsync_NonPendingStatus_Throws()
    {
        var leId = Guid.NewGuid();
        var item = WriteServiceTestBase.SeedAgendaItem(_ctx, leId, status: "REJECTED");

        var act = () => _sut.RejectAsync(item.Id, Guid.NewGuid(), leId, null);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Невозможно отклонить*");
    }

    [Fact]
    public async Task RejectAsync_WithShareRequest_SetsShareRequestRejected()
    {
        var leId = Guid.NewGuid();
        var okopfId = WriteServiceTestBase.SeedOkopf(_ctx);
        var le = WriteServiceTestBase.SeedLegalEntity(_ctx, okopfId, "ООО Тест");
        var user = WriteServiceTestBase.SeedUser(_ctx);
        var bp = WriteServiceTestBase.SeedBoardParticipant(_ctx, le.Id);
        var rt = WriteServiceTestBase.SeedRequestType(_ctx, "EXIT_APPLICATION");
        var sr = WriteServiceTestBase.SeedShareRequest(_ctx, le.Id, bp.Id, rt.Id, "submitted");

        var item = WriteServiceTestBase.SeedAgendaItem(_ctx, le.Id, sr.Id, "PENDING");

        await _sut.RejectAsync(item.Id, Guid.NewGuid(), le.Id, "Отклонено");

        Refresh();
        var updatedSr = _ctx.ShareRequests.FirstOrDefault(r => r.Id == sr.Id);
        updatedSr.Should().NotBeNull();
        updatedSr!.Status.Should().Be("rejected");
        updatedSr.CompletedAt.Should().NotBeNull();
    }
}
