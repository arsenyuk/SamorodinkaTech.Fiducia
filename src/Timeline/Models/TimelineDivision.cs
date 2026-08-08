namespace SamorodinkaTech.Fiducia.Timeline.Models;

/// <summary>Одно деление шкалы (верхнего или нижнего ряда).</summary>
public sealed record TimelineDivision
{
    /// <summary>Начальная дата интервала.</summary>
    public DateOnly Start { get; init; }

    /// <summary>Конечная дата интервала (включительно).</summary>
    public DateOnly End { get; init; }

    /// <summary>Количество ячеек нижнего уровня, которые охватывает данное деление (только для верхнего ряда).</summary>
    public int Span { get; init; } = 1;

    /// <summary>Короткая подпись.</summary>
    public string Label { get; init; } = "";

    /// <summary>Текст всплывающей подсказки.</summary>
    public string Tooltip { get; init; } = "";

    /// <summary>Выходной день (сб/вс) — только для масштаба Days.</summary>
    public bool IsWeekend { get; init; }

    /// <summary>Праздничный день из переданного списка.</summary>
    public bool IsHoliday { get; init; }

    /// <summary>Содержит дату «сегодня».</summary>
    public bool IsToday { get; init; }
}