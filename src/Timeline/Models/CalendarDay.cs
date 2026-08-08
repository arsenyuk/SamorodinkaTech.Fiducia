namespace SamorodinkaTech.Fiducia.Timeline.Models;

/// <summary>Один день производственного календаря со всеми атрибутами.</summary>
public sealed record CalendarDay
{
    /// <summary>Дата.</summary>
    public DateOnly Date { get; init; }

    /// <summary>Рабочий день (не выходной и не праздничный).</summary>
    public bool IsWorkingDay { get; init; }

    /// <summary>Выходной день (суббота или воскресенье).</summary>
    public bool IsWeekend { get; init; }

    /// <summary>Праздничный день (ст. 112 ТК РФ + переносы).</summary>
    public bool IsHoliday { get; init; }

    /// <summary>Номер недели в году (1–53).</summary>
    public int WeekNumber { get; init; }

    /// <summary>Номер месяца (1–12).</summary>
    public int MonthNumber { get; init; }

    /// <summary>Номер квартала (1–4).</summary>
    public int Quarter { get; init; }

    /// <summary>Год.</summary>
    public int Year { get; init; }
}
