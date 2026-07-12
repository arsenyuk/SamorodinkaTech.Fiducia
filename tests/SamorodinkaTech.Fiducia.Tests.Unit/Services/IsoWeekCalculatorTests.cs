using FluentAssertions;
using SamorodinkaTech.Fiducia.Timeline.Services;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Services;

public class IsoWeekCalculatorTests
{
    [Fact]
    public void IsoWeekOf_Jan4_ReturnsWeek1()
    {
        // 4 января всегда в первой ISO-неделе года
        var date = new DateOnly(2025, 1, 4);

        var (isoYear, isoWeek) = IsoWeekCalculator.IsoWeekOf(date);

        isoYear.Should().Be(2025);
        isoWeek.Should().Be(1);
    }

    [Fact]
    public void IsoWeekOf_Dec31_2024_ReturnsWeek1_2025()
    {
        // 31 декабря 2024 (вторник) — может относиться к неделе 1 2025 года
        var date = new DateOnly(2024, 12, 31);

        var (isoYear, isoWeek) = IsoWeekCalculator.IsoWeekOf(date);

        // 31 дек 2024 — вторник, четверг этой недели = 2 янв 2025 → isoYear = 2025
        isoYear.Should().Be(2025);
        isoWeek.Should().Be(1);
    }

    [Fact]
    public void IsoWeekOf_Monday_ReturnsCorrectWeek()
    {
        // 6 января 2025 — понедельник
        var date = new DateOnly(2025, 1, 6);

        var (isoYear, isoWeek) = IsoWeekCalculator.IsoWeekOf(date);

        isoYear.Should().Be(2025);
        isoWeek.Should().Be(2);
    }

    [Fact]
    public void IsoWeekOf_Sunday_ReturnsCorrectWeek()
    {
        // 12 января 2025 — воскресенье
        var date = new DateOnly(2025, 1, 12);

        var (isoYear, isoWeek) = IsoWeekCalculator.IsoWeekOf(date);

        isoYear.Should().Be(2025);
        isoWeek.Should().Be(2);
    }

    [Fact]
    public void IsoWeekKey_ReturnsYearTimes100PlusWeek()
    {
        var date = new DateOnly(2025, 1, 6); // week 2

        var key = IsoWeekCalculator.IsoWeekKey(date);

        key.Should().Be(202502);
    }
}
