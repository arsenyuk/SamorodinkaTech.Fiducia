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

public class CeoResignationServiceTests : IDisposable
{
    private readonly Infrastructure.Persistence.FiduciaDbContext _ctx;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<ICeoResignationDocxGenerator> _docxGeneratorMock;
    private readonly Mock<IFileStorage> _fileStorageMock;
    private readonly CeoResignationService _sut;

    public CeoResignationServiceTests()
    {
        var (factory, _) = WriteServiceTestBase.CreateMockFactory(out _ctx);
        _notificationServiceMock = new Mock<INotificationService>();
        _docxGeneratorMock = new Mock<ICeoResignationDocxGenerator>();
        _fileStorageMock = new Mock<IFileStorage>();
        var textBuilder = new NotificationTextBuilder(_ctx);
        var logger = Mock.Of<ILogger<CeoResignationService>>();

        _docxGeneratorMock
            .Setup(g => g.GenerateAsync(It.IsAny<CeoResignationData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new byte[] { 0x50, 0x4B, 0x03, 0x04 });

        _fileStorageMock
            .Setup(s => s.SaveAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("storage-key-789");

        _sut = new CeoResignationService(factory, _notificationServiceMock.Object, textBuilder,
            _docxGeneratorMock.Object, _fileStorageMock.Object, logger);
    }

    public void Dispose() => _ctx.Dispose();

    private void Refresh() => _ctx.ChangeTracker.Clear();

    [Fact]
    public async Task SendAsync_ValidRequest_CreatesNotifications()
    {
        var okopfId = WriteServiceTestBase.SeedOkopf(_ctx, "12300");
        var le = WriteServiceTestBase.SeedLegalEntity(_ctx, okopfId);

        var ceoUser = WriteServiceTestBase.SeedUser(_ctx, "ceo");
        var ecoPerson = new EcosystemPerson
        {
            Id = Guid.NewGuid(),
            LastName = "ГДов",
            FirstName = "Генеральный",
            CreatedBy = ceoUser.Id
        };
        _ctx.EcosystemPersons.Add(ecoPerson);
        _ctx.SaveChanges();

        var ceoEco = new EcosystemParticipant
        {
            Id = Guid.NewGuid(),
            LegalEntityId = le.Id,
            EcosystemPersonId = ecoPerson.Id,
            UserId = ceoUser.Id,
            IsActive = true
        };
        _ctx.EcosystemParticipants.Add(ceoEco);
        _ctx.SaveChanges();

        var ceoPerson = WriteServiceTestBase.SeedPerson(_ctx, "ГДов", "Генеральный");
        var ceoParticipant = WriteServiceTestBase.SeedBoardParticipant(
            _ctx, le.Id, "FL", true, ceoEco.Id, isGeneralDirector: true);
        ceoParticipant.PersonId = ceoPerson.Id;
        _ctx.SaveChanges();

        var otherUser = WriteServiceTestBase.SeedUser(_ctx, "member");
        var otherEcoPerson = new EcosystemPerson
        {
            Id = Guid.NewGuid(),
            LastName = "Директор",
            FirstName = "Алексей",
            CreatedBy = otherUser.Id
        };
        _ctx.EcosystemPersons.Add(otherEcoPerson);
        _ctx.SaveChanges();

        var otherEco = new EcosystemParticipant
        {
            Id = Guid.NewGuid(),
            LegalEntityId = le.Id,
            EcosystemPersonId = otherEcoPerson.Id,
            UserId = otherUser.Id,
            IsActive = true
        };
        _ctx.EcosystemParticipants.Add(otherEco);
        _ctx.SaveChanges();

        var otherParticipant = WriteServiceTestBase.SeedBoardParticipant(
            _ctx, le.Id, "FL", true, otherEco.Id);
        var otherPerson = WriteServiceTestBase.SeedPerson(_ctx, "Директор", "Алексей");
        otherParticipant.PersonId = otherPerson.Id;
        _ctx.SaveChanges();

        var rt = WriteServiceTestBase.SeedRequestType(_ctx, "DEMAND_VOSU");
        var resignationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(60));

        var model = new CeoResignationSendModel
        {
            LegalEntityId = le.Id,
            UserId = ceoUser.Id,
            ResignationDate = resignationDate,
            ReviewLocation = "г. Москва"
        };

        var result = await _sut.SendAsync(model);

        Refresh();
        result.SentCount.Should().BeGreaterThanOrEqualTo(1);
        result.FileCount.Should().BeGreaterThanOrEqualTo(1);
        result.ResignationDate.Should().Be(resignationDate);
        result.DaysUntilResignation.Should().BeGreaterThanOrEqualTo(30);

        _ctx.Files.Count().Should().BeGreaterThanOrEqualTo(1);

        var shareRequest = _ctx.ShareRequests
            .FirstOrDefault(sr => sr.Status == "submitted" && sr.LegalEntityId == le.Id);
        shareRequest.Should().NotBeNull();
        shareRequest!.Payload.Should().Contain("CEO_RESIGNATION");
    }

    [Fact]
    public async Task SendAsync_TooCloseToResignation_Throws()
    {
        var okopfId = WriteServiceTestBase.SeedOkopf(_ctx, "12300");
        var le = WriteServiceTestBase.SeedLegalEntity(_ctx, okopfId);
        var ceoUser = WriteServiceTestBase.SeedUser(_ctx, "ceo");

        var ecoPerson = new EcosystemPerson
        {
            Id = Guid.NewGuid(),
            LastName = "ГДов",
            FirstName = "Генеральный",
            CreatedBy = ceoUser.Id
        };
        _ctx.EcosystemPersons.Add(ecoPerson);
        _ctx.SaveChanges();

        var ceoEco = new EcosystemParticipant
        {
            Id = Guid.NewGuid(),
            LegalEntityId = le.Id,
            EcosystemPersonId = ecoPerson.Id,
            UserId = ceoUser.Id,
            IsActive = true
        };
        _ctx.EcosystemParticipants.Add(ceoEco);
        _ctx.SaveChanges();

        var ceoPerson = WriteServiceTestBase.SeedPerson(_ctx, "ГДов", "Генеральный");
        var ceoParticipant = WriteServiceTestBase.SeedBoardParticipant(
            _ctx, le.Id, "FL", true, ceoEco.Id, isGeneralDirector: true);
        ceoParticipant.PersonId = ceoPerson.Id;
        _ctx.SaveChanges();

        var resignationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10));
        var model = new CeoResignationSendModel
        {
            LegalEntityId = le.Id,
            UserId = ceoUser.Id,
            ResignationDate = resignationDate
        };

        var act = () => _sut.SendAsync(model);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*не менее 30*");
    }

    [Fact]
    public async Task SendAsync_NotCeo_Throws()
    {
        var okopfId = WriteServiceTestBase.SeedOkopf(_ctx, "12300");
        var le = WriteServiceTestBase.SeedLegalEntity(_ctx, okopfId);
        var user = WriteServiceTestBase.SeedUser(_ctx, "regular");
        var eco = WriteServiceTestBase.SeedEcosystemParticipant(_ctx, user.Id, le.Id);
        _ = WriteServiceTestBase.SeedBoardParticipant(_ctx, le.Id, "FL", true, eco.Id, isGeneralDirector: false);

        var model = new CeoResignationSendModel
        {
            LegalEntityId = le.Id,
            UserId = user.Id,
            ResignationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(60))
        };

        var act = () => _sut.SendAsync(model);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*не является Генеральным директором*");
    }
}
