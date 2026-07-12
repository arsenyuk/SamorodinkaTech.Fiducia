namespace SamorodinkaTech.Fiducia.Timeline.Models;

/// <summary>Аргументы события клика по делению шкалы.</summary>
public sealed class DivisionClickEventArgs
{
    /// <summary>Начальная дата интервала.</summary>
    public DateOnly Start { get; init; }

    /// <summary>Конечная дата интервала (включительно).</summary>
    public DateOnly End { get; init; }

    /// <summary>Тип единицы (масштаб деления).</summary>
    public TimelineScale UnitType { get; init; }
}