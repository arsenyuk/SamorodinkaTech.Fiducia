using FluentAssertions;
using Moq;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Infrastructure.Services;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Services;

public class LegalEntityGosaIntervalServiceTests
{
    private readonly Mock<ITimeProvider> _timeProviderMock = new();
    private readonly LegalEntityGosaIntervalService _sut;

    public LegalEntityGosaIntervalServiceTests()
    {
        _timeProviderMock.Setup(x => x.UtcNow).Returns(new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc));
        _sut = new LegalEntityGosaIntervalService(_timeProviderMock.Object);
    }

    [Fact]
    public void IsPjsc_PjscCode_ReturnsTrue()
    {
        _sut.IsPjsc("12247").Should().BeTrue();
    }

    [Fact]
    public void IsLlc_LlcCode_ReturnsTrue()
    {
        _sut.IsLlc("12300").Should().BeTrue();
    }

    [Fact]
    public void GetDefaultWindow_ReturnsMarchToJune()
    {
        var (start, end) = _sut.GetDefaultWindow();

        start.Should().Be(new DateOnly(2025, 3, 1));
        end.Should().Be(new DateOnly(2025, 6, 30));
    }

    [Fact]
    public void GetWindowForOkopf_Llc_ReturnsMarchToApril()
    {
        var (start, end) = _sut.GetWindowForOkopf("12300");

        start.Should().Be(new DateOnly(2025, 3, 1));
        end.Should().Be(new DateOnly(2025, 4, 30));
    }

    [Fact]
    public void GetWindowForOkopf_Pjsc_ReturnsMarchToJune()
    {
        var (start, end) = _sut.GetWindowForOkopf("12247");

        start.Should().Be(new DateOnly(2025, 3, 1));
        end.Should().Be(new DateOnly(2025, 6, 30));
    }

    [Fact]
    public void ValidateForOkopf_Pjsc_ValidWindow_ReturnsTrue()
    {
        var result = _sut.ValidateForOkopf("12247",
            new DateOnly(2025, 3, 1),
            new DateOnly(2025, 6, 30));

        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateForOkopf_Pjsc_WindowOutsideRange_ReturnsFalse()
    {
        // PJSC allows any window within March 1 - June 30
        // This window starts before March 1
        var result = _sut.ValidateForOkopf("12247",
            new DateOnly(2025, 2, 15),
            new DateOnly(2025, 6, 30));

        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateForOkopf_Llc_ExactWindow_ReturnsTrue()
    {
        var result = _sut.ValidateForOkopf("12300",
            new DateOnly(2025, 3, 1),
            new DateOnly(2025, 4, 30));

        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateForOkopf_Llc_ShiftedWindow_ReturnsFalse()
    {
        var result = _sut.ValidateForOkopf("12300",
            new DateOnly(2025, 3, 15),
            new DateOnly(2025, 5, 15));

        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateForOkopf_EndBeforeStart_ReturnsFalse()
    {
        var result = _sut.ValidateForOkopf("12247",
            new DateOnly(2025, 6, 30),
            new DateOnly(2025, 3, 1));

        result.Should().BeFalse();
    }
}
