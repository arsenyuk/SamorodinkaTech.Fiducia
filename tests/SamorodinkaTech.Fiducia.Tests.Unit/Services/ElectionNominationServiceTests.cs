using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Infrastructure.Services;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Services;

public class ElectionNominationServiceTests
{
    private readonly Mock<IApplicationDbContext> _contextMock = new();
    private readonly Mock<INotificationService> _notificationServiceMock = new();
    private readonly Mock<ILogger<ElectionNominationService>> _loggerMock = new();

    [Fact]
    public void Constructor_NullContext_ThrowsArgumentNullException()
    {
        Action act = () => new ElectionNominationService(
            null!,
            _notificationServiceMock.Object,
            _loggerMock.Object);

        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("context");
    }

    [Fact]
    public void Constructor_NullNotificationService_ThrowsArgumentNullException()
    {
        Action act = () => new ElectionNominationService(
            _contextMock.Object,
            null!,
            _loggerMock.Object);

        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("notificationService");
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        Action act = () => new ElectionNominationService(
            _contextMock.Object,
            _notificationServiceMock.Object,
            null!);

        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("logger");
    }

    [Fact]
    public void Constructor_ValidParameters_CreatesInstance()
    {
        var sut = new ElectionNominationService(
            _contextMock.Object,
            _notificationServiceMock.Object,
            _loggerMock.Object);

        sut.Should().NotBeNull();
    }
}
