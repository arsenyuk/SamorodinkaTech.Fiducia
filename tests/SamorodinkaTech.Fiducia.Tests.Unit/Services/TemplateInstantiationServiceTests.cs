using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SamorodinkaTech.Fiducia.Infrastructure.Services;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Services;

public class TemplateInstantiationServiceTests
{
    private readonly Mock<ILogger<TemplateInstantiationService>> _loggerMock = new();
    private readonly TemplateInstantiationService _sut;

    public TemplateInstantiationServiceTests()
    {
        _sut = new TemplateInstantiationService(_loggerMock.Object);
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        Action act = () => new TemplateInstantiationService(null!);

        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("logger");
    }

    [Fact]
    public void Constructor_ValidLogger_CreatesInstance()
    {
        var sut = new TemplateInstantiationService(_loggerMock.Object);

        sut.Should().NotBeNull();
    }
}
