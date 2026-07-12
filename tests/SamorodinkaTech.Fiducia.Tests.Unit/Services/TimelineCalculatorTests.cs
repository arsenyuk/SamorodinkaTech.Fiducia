using FluentAssertions;
using SamorodinkaTech.Fiducia.Timeline.Models;
using SamorodinkaTech.Fiducia.Timeline.Services;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Services;

public class TimelineCalculatorTests
{
    [Fact]
    public void Compute_DaysScale_ReturnsDailyDivisions()
    {
        var input = new TimelineInput
        {
            Scale = TimelineScale.Days,
            StartDate = new DateOnly(2025, 1, 1),
            EndDate = new DateOnly(2025, 1, 5),
            Today = new DateOnly(2025, 1, 3),
            Holidays = new HashSet<DateOnly>()
        };

        var result = TimelineCalculator.Compute(input);

        result.Lower.Should().HaveCount(5);
        result.Lower[0].Label.Should().Be("1 янв");
        result.Lower[4].Label.Should().Be("5 янв");
    }

    [Fact]
    public void Compute_EmptyRange_ReturnsEmpty()
    {
        var input = new TimelineInput
        {
            Scale = TimelineScale.Days,
            StartDate = new DateOnly(2025, 1, 5),
            EndDate = new DateOnly(2025, 1, 1),
            Today = new DateOnly(2025, 1, 3),
            Holidays = new HashSet<DateOnly>()
        };

        var result = TimelineCalculator.Compute(input);

        result.Lower.Should().BeEmpty();
    }

    [Fact]
    public void Compute_WeeksScale_ReturnsWeeklyDivisions()
    {
        var input = new TimelineInput
        {
            Scale = TimelineScale.Weeks,
            StartDate = new DateOnly(2025, 1, 1),
            EndDate = new DateOnly(2025, 1, 31),
            Today = new DateOnly(2025, 1, 15),
            Holidays = new HashSet<DateOnly>()
        };

        var result = TimelineCalculator.Compute(input);

        result.Lower.Should().NotBeEmpty();
        result.Lower.All(d => d.Span == 1).Should().BeTrue();
    }

    [Fact]
    public void Compute_MonthsScale_ReturnsMonthlyDivisions()
    {
        var input = new TimelineInput
        {
            Scale = TimelineScale.Months,
            StartDate = new DateOnly(2025, 1, 1),
            EndDate = new DateOnly(2025, 6, 30),
            Today = new DateOnly(2025, 3, 15),
            Holidays = new HashSet<DateOnly>()
        };

        var result = TimelineCalculator.Compute(input);

        result.Lower.Should().HaveCount(6);
        result.Lower[0].Label.Should().Be("янв");
        result.Lower[5].Label.Should().Be("июн");
    }

    [Fact]
    public void ComputeAllLevels_ReturnsAllLevels()
    {
        var input = new TimelineInput
        {
            Scale = TimelineScale.Days,
            StartDate = new DateOnly(2025, 1, 1),
            EndDate = new DateOnly(2025, 3, 31),
            Today = new DateOnly(2025, 2, 15),
            Holidays = new HashSet<DateOnly>()
        };

        var result = TimelineCalculator.ComputeAllLevels(input);

        result.Days.Should().NotBeEmpty();
        result.Weeks.Should().NotBeEmpty();
        result.Months.Should().NotBeEmpty();
        result.Quarters.Should().NotBeEmpty();
        result.Years.Should().NotBeEmpty();
    }
}
