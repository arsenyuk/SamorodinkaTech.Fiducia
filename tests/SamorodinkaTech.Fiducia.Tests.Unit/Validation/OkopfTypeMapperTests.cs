using FluentAssertions;
using SamorodinkaTech.Fiducia.Domain.Validation;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Validation;

public class OkopfTypeMapperTests
{
    // ── DetectType ────────────────────────────────────────────────

    [Fact]
    public void DetectType_PjscCode_ReturnsPJSC()
    {
        OkopfTypeMapper.DetectType("12247").Should().Be(OrgValidationType.PJSC);
    }

    [Fact]
    public void DetectType_NjscCode_ReturnsNJSC()
    {
        OkopfTypeMapper.DetectType("12267").Should().Be(OrgValidationType.NJSC);
    }

    [Fact]
    public void DetectType_LlcCode_ReturnsLLC()
    {
        OkopfTypeMapper.DetectType("12300").Should().Be(OrgValidationType.LLC);
    }

    [Fact]
    public void DetectType_UnknownCode_ReturnsUnknown()
    {
        OkopfTypeMapper.DetectType("99999").Should().Be(OrgValidationType.Unknown);
    }

    [Fact]
    public void DetectType_Null_ReturnsUnknown()
    {
        OkopfTypeMapper.DetectType(null).Should().Be(OrgValidationType.Unknown);
    }

    [Fact]
    public void DetectType_Empty_ReturnsUnknown()
    {
        OkopfTypeMapper.DetectType("").Should().Be(OrgValidationType.Unknown);
    }

    [Fact]
    public void DetectType_CodeWithSpaces_StillWorks()
    {
        OkopfTypeMapper.DetectType("12 247").Should().Be(OrgValidationType.PJSC);
    }

    // ── IsPjsc ────────────────────────────────────────────────────

    [Fact]
    public void IsPjsc_PjscCode_ReturnsTrue()
    {
        OkopfTypeMapper.IsPjsc("12247").Should().BeTrue();
    }

    [Fact]
    public void IsPjsc_LlcCode_ReturnsFalse()
    {
        OkopfTypeMapper.IsPjsc("12300").Should().BeFalse();
    }

    [Fact]
    public void IsPjsc_Null_ReturnsFalse()
    {
        OkopfTypeMapper.IsPjsc(null).Should().BeFalse();
    }

    // ── IsLlc ─────────────────────────────────────────────────────

    [Fact]
    public void IsLlc_LlcCode_ReturnsTrue()
    {
        OkopfTypeMapper.IsLlc("12300").Should().BeTrue();
    }

    [Fact]
    public void IsLlc_PjscCode_ReturnsFalse()
    {
        OkopfTypeMapper.IsLlc("12247").Should().BeFalse();
    }

    // ── TypeLabel ─────────────────────────────────────────────────

    [Theory]
    [InlineData(OrgValidationType.PJSC, "ПАО")]
    [InlineData(OrgValidationType.NJSC, "непубличного АО")]
    [InlineData(OrgValidationType.LLC, "ООО")]
    [InlineData(OrgValidationType.Unknown, "данного типа общества")]
    public void TypeLabel_ReturnsCorrectLabel(OrgValidationType type, string expected)
    {
        OkopfTypeMapper.TypeLabel(type).Should().Be(expected);
    }
}
