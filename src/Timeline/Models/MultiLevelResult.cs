namespace SamorodinkaTech.Fiducia.Timeline.Models;

/// <summary>Результат вычисления всех уровней календарной шкалы.</summary>
public sealed class MultiLevelResult
{
    /// <summary>Деления уровня «Годы».</summary>
    public IReadOnlyList<TimelineDivision> Years { get; init; } = [];

    /// <summary>Деления уровня «Кварталы».</summary>
    public IReadOnlyList<TimelineDivision> Quarters { get; init; } = [];

    /// <summary>Деления уровня «Месяцы».</summary>
    public IReadOnlyList<TimelineDivision> Months { get; init; } = [];

    /// <summary>Деления уровня «Недели».</summary>
    public IReadOnlyList<TimelineDivision> Weeks { get; init; } = [];

    /// <summary>Деления уровня «Дни».</summary>
    public IReadOnlyList<TimelineDivision> Days { get; init; } = [];

    /// <summary>Позиция отметки «сегодня» в пикселях от начала шкалы (дни — базовый уровень). -1 если сегодня вне диапазона.</summary>
    public double TodayPixel { get; init; } = -1;
}