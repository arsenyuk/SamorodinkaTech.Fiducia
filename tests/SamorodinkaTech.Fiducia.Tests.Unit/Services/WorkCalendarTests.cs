using FluentAssertions;
using SamorodinkaTech.Fiducia.Timeline.Services;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Services;

public class WorkCalendarTests
{
    // ── GetHolidays ───────────────────────────────────────────────

    [Fact]
    public void GetHolidays_2025_ContainsNewYear()
    {
        var holidays = WorkCalendar.GetHolidays(2025);

        holidays.Should().Contain(new DateOnly(2025, 1, 1));
        holidays.Should().Contain(new DateOnly(2025, 1, 8));
    }

    [Fact]
    public void GetHolidays_2025_ContainsFixedHolidays()
    {
        var holidays = WorkCalendar.GetHolidays(2025);

        holidays.Should().Contain(new DateOnly(2025, 2, 23)); // День защитника Отечества
        // 8 марта 2025 — суббота, перенесена на 13 июня
        holidays.Should().Contain(new DateOnly(2025, 5, 1));  // Праздник Весны и Труда
        holidays.Should().Contain(new DateOnly(2025, 5, 9));  // День Победы
        holidays.Should().Contain(new DateOnly(2025, 6, 12)); // День России
        holidays.Should().Contain(new DateOnly(2025, 11, 4)); // День народного единства
    }

    [Fact]
    public void GetHolidays_2025_ContainsTransfers()
    {
        var holidays = WorkCalendar.GetHolidays(2025);

        // Переносы по постановлению Правительства РФ от 04.10.2024
        holidays.Should().Contain(new DateOnly(2025, 5, 2));  // 4 янв (сб) → 2 мая (пт)
        holidays.Should().Contain(new DateOnly(2025, 5, 8));  // 23 фев (вс) → 8 мая (чт)
        holidays.Should().Contain(new DateOnly(2025, 6, 13)); // 8 мар (сб) → 13 июня (пт)

        // 4 января стала рабочей
        holidays.Should().NotContain(new DateOnly(2025, 1, 4));
        // 8 марта стала рабочей (суббота)
        holidays.Should().NotContain(new DateOnly(2025, 3, 8));
    }

    [Fact]
    public void GetHolidays_2025_MinCount()
    {
        var holidays = WorkCalendar.GetHolidays(2025);

        // Минимум 14 нерабочих дней (базовые праздники минус перенесённые + добавленные)
        holidays.Count.Should().BeGreaterThanOrEqualTo(14);
    }

    // ── IsNonWorking ──────────────────────────────────────────────

    [Fact]
    public void IsNonWorking_Saturday_ReturnsTrue()
    {
        var saturday = new DateOnly(2025, 1, 4); // суббота
        WorkCalendar.IsNonWorking(saturday).Should().BeTrue();
    }

    [Fact]
    public void IsNonWorking_Sunday_ReturnsTrue()
    {
        var sunday = new DateOnly(2025, 1, 5); // воскресенье
        WorkCalendar.IsNonWorking(sunday).Should().BeTrue();
    }

    [Fact]
    public void IsNonWorking_Weekday_ReturnsFalse()
    {
        var monday = new DateOnly(2025, 1, 6); // понедельник (праздник, но не выходной)
        var holidays = WorkCalendar.GetHolidays(2025);

        // 6 января — праздничный день, проверяем обычный рабочий день
        var tuesday = new DateOnly(2025, 1, 14); // вторник
        WorkCalendar.IsNonWorking(tuesday, holidays).Should().BeFalse();
    }

    [Fact]
    public void IsNonWorking_Holiday_ReturnsTrue()
    {
        var holidays = new HashSet<DateOnly> { new DateOnly(2025, 3, 8) };
        var date = new DateOnly(2025, 3, 8); // суббота

        WorkCalendar.IsNonWorking(date, holidays).Should().BeTrue();
    }

    [Fact]
    public void IsNonWorking_HolidayOnWeekday_ReturnsTrue()
    {
        var holidays = new HashSet<DateOnly> { new DateOnly(2025, 5, 1) };
        var date = new DateOnly(2025, 5, 1); // четверг, праздник

        WorkCalendar.IsNonWorking(date, holidays).Should().BeTrue();
    }

    // ── GetNextWorkingDay ─────────────────────────────────────────

    [Fact]
    public void GetNextWorkingDay_FromHoliday_ReturnsNextDay()
    {
        var holidays = new HashSet<DateOnly> { new DateOnly(2025, 1, 1) };
        var date = new DateOnly(2025, 1, 1); // среда, праздник

        var result = WorkCalendar.GetNextWorkingDay(date, holidays);

        result.Should().Be(new DateOnly(2025, 1, 2)); // четверг
    }

    [Fact]
    public void GetNextWorkingDay_FromWeekend_ReturnsMonday()
    {
        var saturday = new DateOnly(2025, 1, 4);

        var result = WorkCalendar.GetNextWorkingDay(saturday);

        result.Should().Be(new DateOnly(2025, 1, 6)); // понедельник (праздник, но проверяем логику)
    }

    [Fact]
    public void GetNextWorkingDay_FromWorkday_ReturnsSameDay()
    {
        var tuesday = new DateOnly(2025, 1, 14);

        var result = WorkCalendar.GetNextWorkingDay(tuesday);

        result.Should().Be(tuesday);
    }

    // ── GenerateCalendar ──────────────────────────────────────────

    [Fact]
    public void GenerateCalendar_SingleDay_ReturnsOneEntry()
    {
        var calendar = WorkCalendar.GenerateCalendar(2025, 2025);

        calendar.Should().NotBeEmpty();
        calendar.First().Date.Should().Be(new DateOnly(2025, 1, 1));
        calendar.Last().Date.Should().Be(new DateOnly(2025, 12, 31));
        calendar.Count.Should().Be(365);
    }
}
