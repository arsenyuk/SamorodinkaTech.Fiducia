using FluentAssertions;
using SamorodinkaTech.Fiducia.Infrastructure.Services;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Services;

public class WorkingDayHelperTests
{
    [Fact]
    public void IsNonWorking_Saturday_ReturnsTrue()
    {
        var saturday = new DateOnly(2025, 1, 4); // суббота

        WorkingDayHelper.IsNonWorking(saturday).Should().BeTrue();
    }

    [Fact]
    public void IsNonWorking_Sunday_ReturnsTrue()
    {
        var sunday = new DateOnly(2025, 1, 5); // воскресенье

        WorkingDayHelper.IsNonWorking(sunday).Should().BeTrue();
    }

    [Fact]
    public void IsNonWorking_Weekday_ReturnsFalse()
    {
        var tuesday = new DateOnly(2025, 1, 14); // вторник

        WorkingDayHelper.IsNonWorking(tuesday).Should().BeFalse();
    }

    [Fact]
    public void GetHolidays_2025_ContainsNewYear()
    {
        var holidays = WorkingDayHelper.GetHolidays(2025);

        holidays.Should().Contain(new DateOnly(2025, 1, 1));
        holidays.Should().Contain(new DateOnly(2025, 1, 8));
    }

    [Fact]
    public void GetNextWorkingDay_FromWorkday_ReturnsSameDay()
    {
        var tuesday = new DateOnly(2025, 1, 14);

        var result = WorkingDayHelper.GetNextWorkingDay(tuesday);

        result.Should().Be(tuesday);
    }

    [Fact]
    public void GetNextWorkingDay_FromHoliday_ReturnsNextWorkingDay()
    {
        var holidays = new HashSet<DateOnly> { new DateOnly(2025, 1, 1) };
        var date = new DateOnly(2025, 1, 1); // среда, праздник

        var result = WorkingDayHelper.GetNextWorkingDay(date, holidays);

        result.Should().Be(new DateOnly(2025, 1, 2)); // четверг
    }
}
