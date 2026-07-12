namespace SamorodinkaTech.Fiducia.Timeline.Models;

/// <summary>Входные данные для инициализации/обновления шкалы.</summary>
public sealed record TimelineInput
{
    /// <summary>Начальная дата отображаемого периода.</summary>
    public DateOnly StartDate { get; init; }

    /// <summary>Конечная дата отображаемого периода (включительно).</summary>
    public DateOnly EndDate { get; init; }

    /// <summary>Дата «сегодня» (для отметки на шкале).</summary>
    public DateOnly Today { get; init; }

    /// <summary>Текущий масштаб нижнего ряда.</summary>
    public TimelineScale Scale { get; init; }

    /// <summary>Множество праздничных дат (опционально).</summary>
    public IReadOnlySet<DateOnly> Holidays { get; init; } = new HashSet<DateOnly>();

    /// <summary>Предвычисленный календарь дней с атрибутами (год, квартал, месяц, неделя, рабочий/выходной/праздничный).</summary>
    public IReadOnlyList<CalendarDay> CalendarDays { get; init; } = [];
}