using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Infrastructure.Services;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Services;

public class ShareRequestWriteServiceTests : IDisposable
{
    private readonly Infrastructure.Persistence.FiduciaDbContext _ctx;
    private readonly ShareRequestWriteService _sut;

    public ShareRequestWriteServiceTests()
    {
        var (factory, _) = WriteServiceTestBase.CreateMockFactory(out _ctx);
        var logger = Mock.Of<ILogger<ShareRequestWriteService>>();
        var templateService = Mock.Of<ITemplateInstantiationService>();
        var docProvision = Mock.Of<IDocumentProvisionService>();
        var vosuDocx = Mock.Of<IVosuNotificationDocxGenerator>();
        var fileStorage = Mock.Of<IFileStorage>();
        _sut = new ShareRequestWriteService(factory, logger, templateService, docProvision, vosuDocx, fileStorage);
    }

    public void Dispose() => _ctx.Dispose();

    /// <summary>Очищает ChangeTracker перед assertions, т.к. сервис создаёт собственные контексты.</summary>
    private void Refresh() => _ctx.ChangeTracker.Clear();

    [Fact]
    public async Task SubmitAsync_CompleteRequest_SetsStatusSubmitted()
    {
        var okopfId = WriteServiceTestBase.SeedOkopf(_ctx, "12300");
        var le = WriteServiceTestBase.SeedLegalEntity(_ctx, okopfId);
        var rt = WriteServiceTestBase.SeedRequestType(_ctx, "EXIT_APPLICATION");
        var bp = WriteServiceTestBase.SeedBoardParticipant(_ctx, le.Id);
        var sr = WriteServiceTestBase.SeedShareRequest(_ctx, le.Id, bp.Id, rt.Id, "draft");

        await _sut.SubmitAsync(sr.Id);

        Refresh();
        var updated = _ctx.ShareRequests.FirstOrDefault(r => r.Id == sr.Id);
        updated.Should().NotBeNull();
        updated!.Status.Should().Be("submitted");
    }

    [Fact]
    public async Task SubmitAsync_AlreadySubmitted_Throws()
    {
        var okopfId = WriteServiceTestBase.SeedOkopf(_ctx, "12300");
        var le = WriteServiceTestBase.SeedLegalEntity(_ctx, okopfId);
        var rt = WriteServiceTestBase.SeedRequestType(_ctx, "EXIT_APPLICATION");
        var bp = WriteServiceTestBase.SeedBoardParticipant(_ctx, le.Id);
        var sr = WriteServiceTestBase.SeedShareRequest(_ctx, le.Id, bp.Id, rt.Id, "submitted");

        var act = () => _sut.SubmitAsync(sr.Id);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*уже отправлено*");
    }

    [Fact]
    public async Task DecideAsync_Approve_SetsStatusAccepted()
    {
        var okopfId = WriteServiceTestBase.SeedOkopf(_ctx, "12300");
        var le = WriteServiceTestBase.SeedLegalEntity(_ctx, okopfId);
        var rt = WriteServiceTestBase.SeedRequestType(_ctx, "EXIT_APPLICATION");
        var bp = WriteServiceTestBase.SeedBoardParticipant(_ctx, le.Id);
        var sr = WriteServiceTestBase.SeedShareRequest(_ctx, le.Id, bp.Id, rt.Id, "submitted");

        await _sut.DecideAsync(sr.Id, approved: true, reason: "Согласовано");

        Refresh();
        var updated = _ctx.ShareRequests.FirstOrDefault(r => r.Id == sr.Id);
        updated.Should().NotBeNull();
        updated!.Status.Should().Be("ACCEPTED");
        updated.CeoComment.Should().Be("Согласовано");
        updated.CeoDecisionAt.Should().NotBeNull();
        updated.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DecideAsync_Reject_SetsStatusRejected()
    {
        var okopfId = WriteServiceTestBase.SeedOkopf(_ctx, "12300");
        var le = WriteServiceTestBase.SeedLegalEntity(_ctx, okopfId);
        var rt = WriteServiceTestBase.SeedRequestType(_ctx, "EXIT_APPLICATION");
        var bp = WriteServiceTestBase.SeedBoardParticipant(_ctx, le.Id);
        var sr = WriteServiceTestBase.SeedShareRequest(_ctx, le.Id, bp.Id, rt.Id, "submitted");

        await _sut.DecideAsync(sr.Id, approved: false, reason: "Отклонено");

        Refresh();
        var updated = _ctx.ShareRequests.FirstOrDefault(r => r.Id == sr.Id);
        updated.Should().NotBeNull();
        updated!.Status.Should().Be("REJECTED");
        updated.CeoComment.Should().Be("Отклонено");
    }

    [Fact]
    public async Task RevokeAsync_SubmittedRequest_SetsStatusRevoked()
    {
        var okopfId = WriteServiceTestBase.SeedOkopf(_ctx, "12300");
        var le = WriteServiceTestBase.SeedLegalEntity(_ctx, okopfId);
        var rt = WriteServiceTestBase.SeedRequestType(_ctx, "NOTARIAL_OFFER");
        var bp = WriteServiceTestBase.SeedBoardParticipant(_ctx, le.Id);
        var sr = WriteServiceTestBase.SeedShareRequest(_ctx, le.Id, bp.Id, rt.Id, "submitted");

        await _sut.RevokeAsync(sr.Id, notarized: true);

        Refresh();
        var updated = _ctx.ShareRequests.FirstOrDefault(r => r.Id == sr.Id);
        updated.Should().NotBeNull();
        updated!.Status.Should().Be("revoked");
        updated.RevokedAt.Should().NotBeNull();
        updated.RevokedByNotarized.Should().BeTrue();
    }

    [Fact]
    public async Task SupportAsync_ValidParticipant_AddsSupport()
    {
        var okopfId = WriteServiceTestBase.SeedOkopf(_ctx, "12300");
        var le = WriteServiceTestBase.SeedLegalEntity(_ctx, okopfId);
        var rt = WriteServiceTestBase.SeedRequestType(_ctx, "DEMAND_VOSU");
        var bp1 = WriteServiceTestBase.SeedBoardParticipant(_ctx, le.Id);
        WriteServiceTestBase.SeedShare(_ctx, bp1.Id, le.Id, 30m);

        var sr = WriteServiceTestBase.SeedShareRequest(_ctx, le.Id, bp1.Id, rt.Id, "draft");
        sr.IsCollective = true;
        sr.CollectiveStatus = "COLLECTING";
        sr.ThresholdPercent = 50m;
        _ctx.SaveChanges();

        var bp2 = WriteServiceTestBase.SeedBoardParticipant(_ctx, le.Id);
        WriteServiceTestBase.SeedShare(_ctx, bp2.Id, le.Id, 25m);

        await _sut.SupportAsync(sr.Id, bp2.Id);

        Refresh();
        var support = _ctx.ShareRequestSupports
            .FirstOrDefault(s => s.ShareRequestId == sr.Id && s.ParticipantId == bp2.Id);
        support.Should().NotBeNull();
        support!.SharePercentAtSupport.Should().Be(25m);

        var updatedSr = _ctx.ShareRequests.FirstOrDefault(r => r.Id == sr.Id);
        updatedSr!.TotalSupportPercent.Should().Be(25m);
        updatedSr.SupporterCount.Should().Be(1);
    }

    [Fact]
    public async Task WithdrawAsync_ExistingSupport_RemovesIt()
    {
        var okopfId = WriteServiceTestBase.SeedOkopf(_ctx, "12300");
        var le = WriteServiceTestBase.SeedLegalEntity(_ctx, okopfId);
        var rt = WriteServiceTestBase.SeedRequestType(_ctx, "DEMAND_VOSU");
        var bp1 = WriteServiceTestBase.SeedBoardParticipant(_ctx, le.Id);
        var bp2 = WriteServiceTestBase.SeedBoardParticipant(_ctx, le.Id);
        WriteServiceTestBase.SeedShare(_ctx, bp2.Id, le.Id, 25m);

        var sr = WriteServiceTestBase.SeedShareRequest(_ctx, le.Id, bp1.Id, rt.Id, "draft");
        sr.IsCollective = true;
        sr.CollectiveStatus = "COLLECTING";
        sr.TotalSupportPercent = 25m;
        sr.SupporterCount = 1;
        _ctx.SaveChanges();

        var support = new ShareRequestSupport
        {
            Id = Guid.NewGuid(),
            ShareRequestId = sr.Id,
            ParticipantId = bp2.Id,
            SharePercentAtSupport = 25m,
            SupportedAt = DateTime.UtcNow
        };
        _ctx.ShareRequestSupports.Add(support);
        _ctx.SaveChanges();

        await _sut.WithdrawAsync(sr.Id, bp2.Id);

        Refresh();
        var updatedSupport = _ctx.ShareRequestSupports.FirstOrDefault(s => s.Id == support.Id);
        updatedSupport!.WithdrawnAt.Should().NotBeNull();

        var updatedSr = _ctx.ShareRequests.FirstOrDefault(r => r.Id == sr.Id);
        updatedSr!.TotalSupportPercent.Should().Be(0m);
        updatedSr.SupporterCount.Should().Be(0);
    }

    [Fact]
    public async Task DecideAsync_DraftRequest_Throws()
    {
        var okopfId = WriteServiceTestBase.SeedOkopf(_ctx, "12300");
        var le = WriteServiceTestBase.SeedLegalEntity(_ctx, okopfId);
        var rt = WriteServiceTestBase.SeedRequestType(_ctx, "EXIT_APPLICATION");
        var bp = WriteServiceTestBase.SeedBoardParticipant(_ctx, le.Id);
        var sr = WriteServiceTestBase.SeedShareRequest(_ctx, le.Id, bp.Id, rt.Id, "draft");

        var act = () => _sut.DecideAsync(sr.Id, approved: true, null);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*не может быть рассмотрено*");
    }
}
