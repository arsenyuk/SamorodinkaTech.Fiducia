using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models;
using SamorodinkaTech.Fiducia.Domain.Services;
using SamorodinkaTech.Fiducia.Infrastructure.Services;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Services;

public class VosuNotificationServiceTests : IDisposable
{
    private readonly Infrastructure.Persistence.FiduciaDbContext _ctx;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IVosuNotificationDocxGenerator> _docxGeneratorMock;
    private readonly Mock<IFileStorage> _fileStorageMock;
    private readonly VosuNotificationService _sut;

    public VosuNotificationServiceTests()
    {
        var (factory, _) = WriteServiceTestBase.CreateMockFactory(out _ctx);
        _notificationServiceMock = new Mock<INotificationService>();
        _docxGeneratorMock = new Mock<IVosuNotificationDocxGenerator>();
        _fileStorageMock = new Mock<IFileStorage>();
        var textBuilder = new NotificationTextBuilder(_ctx);
        var logger = Mock.Of<ILogger<VosuNotificationService>>();

        _docxGeneratorMock
            .Setup(g => g.GenerateAsync(It.IsAny<VosuNotificationData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new byte[] { 0x50, 0x4B, 0x03, 0x04 });

        _fileStorageMock
            .Setup(s => s.SaveAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("storage-key-123");

        _sut = new VosuNotificationService(factory, _notificationServiceMock.Object, textBuilder,
            _docxGeneratorMock.Object, _fileStorageMock.Object, logger);
    }

    public void Dispose() => _ctx.Dispose();

    private void Refresh() => _ctx.ChangeTracker.Clear();

    [Fact]
    public async Task SendAsync_ValidMeeting_CreatesNotifications()
    {
        var okopfId = WriteServiceTestBase.SeedOkopf(_ctx, "12300");
        var (meeting, le) = WriteServiceTestBase.SeedMeetingForNotification(_ctx, okopfId);

        var user = WriteServiceTestBase.SeedUser(_ctx, "ceo");
        var eco = WriteServiceTestBase.SeedEcosystemParticipant(_ctx, user.Id, le.Id);
        var bp = WriteServiceTestBase.SeedBoardParticipant(_ctx, le.Id, "FL", true, eco.Id);

        var person = WriteServiceTestBase.SeedPerson(_ctx);
        bp.PersonId = person.Id;
        _ctx.SaveChanges();

        var model = new VosuNotificationSendModel
        {
            MeetingId = meeting.Id,
            LegalEntityId = le.Id,
            UserId = user.Id,
            MeetingDate = new DateOnly(2025, 9, 15),
            MeetingStartTime = new TimeOnly(10, 0),
            MeetingVenue = "г. Москва, ул. Тестовая, 1",
            IsAgendaChange = false
        };

        var fileIds = await _sut.SendAsync(model);

        Refresh();
        fileIds.Should().HaveCount(1);

        var updatedMeeting = _ctx.OsaMeetings.FirstOrDefault(m => m.Id == meeting.Id);
        updatedMeeting!.GosaWindowStart.Should().Be(new DateOnly(2025, 9, 15));
        updatedMeeting.MeetingStartTime.Should().Be(new TimeOnly(10, 0));
        updatedMeeting.MeetingVenue.Should().Be("г. Москва, ул. Тестовая, 1");

        _ctx.Files.Count().Should().Be(1);
        _ctx.OsaMeetingFiles.Count().Should().Be(1);

        _docxGeneratorMock.Verify(g =>
            g.GenerateAsync(It.IsAny<VosuNotificationData>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_MeetingNotFound_Throws()
    {
        var model = new VosuNotificationSendModel
        {
            MeetingId = Guid.NewGuid(),
            LegalEntityId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            MeetingDate = new DateOnly(2025, 9, 15),
            CeoName = "ГД"
        };

        var act = () => _sut.SendAsync(model);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*не найдено*");
    }

    [Fact]
    public async Task SendAsync_NoActiveParticipants_Throws()
    {
        var okopfId = WriteServiceTestBase.SeedOkopf(_ctx, "12300");
        var (meeting, le) = WriteServiceTestBase.SeedMeetingForNotification(_ctx, okopfId);
        var user = WriteServiceTestBase.SeedUser(_ctx);

        var model = new VosuNotificationSendModel
        {
            MeetingId = meeting.Id,
            LegalEntityId = le.Id,
            UserId = user.Id,
            MeetingDate = new DateOnly(2025, 9, 15),
            CeoName = "ГД"
        };

        var act = () => _sut.SendAsync(model);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Нет активных участников*");
    }
}
