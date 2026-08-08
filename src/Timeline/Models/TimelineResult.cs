namespace SamorodinkaTech.Fiducia.Timeline.Models;

/// <summary>Результат вычисления шкалы: оба ряда делений и позиция «сегодня».</summary>
public sealed class TimelineResult
{
    /// <summary>Деления верхнего ряда.</summary>
    public IReadOnlyList<TimelineDivision> Upper { get; }

    /// <summary>Деления нижнего ряда.</summary>
    public IReadOnlyList<TimelineDivision> Lower { get; }

    /// <summary>Позиция отметки «сегодня» в условных единицах-ячейках от начала шкалы (дробная часть — смещение внутри ячейки). -1 если сегодня вне диапазона.</summary>
    public double TodayCellPosition { get; }

    public TimelineResult(IReadOnlyList<TimelineDivision> upper, IReadOnlyList<TimelineDivision> lower, double todayCellPosition)
    {
        Upper = upper;
        Lower = lower;
        TodayCellPosition = todayCellPosition;
    }
}