using FluentAssertions;
using SamorodinkaTech.Fiducia.Infrastructure.Common;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Common;

/// <summary>
/// Unit-тесты ShareParser.Parse и ShareParser.Format.
/// </summary>
public class ShareParserTests
{
    // ─── Позитивные сценарии: дробь ───────────────────────

    [Theory]
    [InlineData("1/3", 33.33)]
    [InlineData("1/4", 25.00)]
    [InlineData("1/2", 50.00)]
    [InlineData("1/1", 100.00)]
    [InlineData("2/5", 40.00)]
    [InlineData("0.5/1", 50.00)]
    [InlineData("1/8", 12.50)]
    public void Parse_Fraction_ReturnsCorrectPercent(string input, decimal expectedPercent)
    {
        var (percent, fraction) = ShareParser.Parse(input);

        percent.Should().Be(expectedPercent);
        fraction.Should().Be(input);
    }

    // ─── Позитивные сценарии: процент ─────────────────────

    [Theory]
    [InlineData("50", 50.00)]
    [InlineData("25.5", 25.50)]
    [InlineData("100", 100.00)]
    [InlineData("0.01", 0.01)]
    [InlineData("50%", 50.00)]
    [InlineData("  12.5  %  ", 12.50)]
    public void Parse_Percent_ReturnsCorrectValue(string input, decimal expectedPercent)
    {
        var (percent, fraction) = ShareParser.Parse(input);

        percent.Should().Be(expectedPercent);
        fraction.Should().BeNull();
    }

    // ─── Негативные: пустое значение ──────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("%")]
    public void Parse_Empty_ThrowsArgumentException(string? input)
    {
        var act = () => ShareParser.Parse(input!);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*не может быть пустым*");
    }

    // ─── Негативные: некорректный формат ───────────────────

    [Theory]
    [InlineData("abc")]
    [InlineData("1.2.3")]
    [InlineData("--5")]
    public void Parse_NonNumeric_ThrowsArgumentException(string input)
    {
        var act = () => ShareParser.Parse(input);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Некорректное*");
    }

    [Theory]
    [InlineData("1/3/4")]
    [InlineData("1/")]
    [InlineData("/3")]
    public void Parse_InvalidFractionFormat_ThrowsArgumentException(string input)
    {
        var act = () => ShareParser.Parse(input);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*формат дроби*");
    }

    // ─── Негативные: знаменатель = 0 ──────────────────────

    [Theory]
    [InlineData("1/0")]
    [InlineData("5/0")]
    public void Parse_ZeroDenominator_ThrowsArgumentException(string input)
    {
        var act = () => ShareParser.Parse(input);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*знаменатель*нулём*");
    }

    // ─── Негативные: дробь = 0 ────────────────────────────

    [Theory]
    [InlineData("0/5")]
    [InlineData("0/1")]
    public void Parse_ZeroFraction_ThrowsArgumentException(string input)
    {
        var act = () => ShareParser.Parse(input);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*равна нулю*");
    }

    // ─── Негативные: процент = 0 ──────────────────────────

    [Theory]
    [InlineData("0")]
    [InlineData("0.00")]
    public void Parse_ZeroPercent_ThrowsArgumentException(string input)
    {
        var act = () => ShareParser.Parse(input);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*равна нулю*");
    }

    // ─── Негативные: отрицательная дробь ───────────────────

    [Theory]
    [InlineData("-1/3")]
    [InlineData("1/-3")]
    [InlineData("-1/-3")]
    public void Parse_NegativeFraction_ThrowsArgumentException(string input)
    {
        var act = () => ShareParser.Parse(input);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*отрицательной*");
    }

    // ─── Негативные: отрицательный процент ─────────────────

    [Theory]
    [InlineData("-5")]
    [InlineData("-0.5")]
    [InlineData("-100")]
    public void Parse_NegativePercent_ThrowsArgumentException(string input)
    {
        var act = () => ShareParser.Parse(input);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*отрицательной*");
    }

    // ─── Негативные: дробь > 1 ────────────────────────────

    [Theory]
    [InlineData("2/1")]
    [InlineData("3/2")]
    [InlineData("100/50")]
    public void Parse_FractionOverOne_ThrowsArgumentException(string input)
    {
        var act = () => ShareParser.Parse(input);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*превышать 100%*");
    }

    // ─── Негативные: процент > 100 ────────────────────────

    [Theory]
    [InlineData("101")]
    [InlineData("150")]
    [InlineData("100.01")]
    public void Parse_PercentOver100_ThrowsArgumentException(string input)
    {
        var act = () => ShareParser.Parse(input);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*превышать 100%*");
    }

    // ─── Format ────────────────────────────────────────────

    [Fact]
    public void Format_WithFraction_ReturnsFraction()
    {
        var result = ShareParser.Format(33.33m, "1/3");

        result.Should().Be("1/3");
    }

    [Fact]
    public void Format_WithoutFraction_ReturnsPercent()
    {
        var result = ShareParser.Format(50m, null);

        result.Should().Be("50%");
    }

    [Fact]
    public void Format_NullValues_ReturnsDash()
    {
        var result = ShareParser.Format(null, null);

        result.Should().Be("—");
    }

    // ══════════════════════════════════════════════════════════════
    // ValidateServer — серверная валидация (идентична клиентской)
    // ══════════════════════════════════════════════════════════════

    [Theory]
    [InlineData(null)]
    public void ValidateServer_Null_ThrowsArgumentException(decimal? value)
    {
        var act = () => ShareParser.ValidateServer(value);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*обязателен*");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void ValidateServer_Negative_ThrowsArgumentException(decimal value)
    {
        var act = () => ShareParser.ValidateServer(value);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*отрицательной*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.00)]
    public void ValidateServer_Zero_ThrowsArgumentException(decimal value)
    {
        var act = () => ShareParser.ValidateServer(value);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*равна нулю*");
    }

    [Theory]
    [InlineData(101)]
    [InlineData(150)]
    [InlineData(100.01)]
    public void ValidateServer_Over100_ThrowsArgumentException(decimal value)
    {
        var act = () => ShareParser.ValidateServer(value);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*превышать 100%*");
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(50)]
    [InlineData(100)]
    public void ValidateServer_Valid_DoesNotThrow(decimal value)
    {
        var act = () => ShareParser.ValidateServer(value);

        act.Should().NotThrow();
    }
}
