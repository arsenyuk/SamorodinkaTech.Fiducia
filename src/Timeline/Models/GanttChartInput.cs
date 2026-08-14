namespace SamorodinkaTech.Fiducia.Timeline.Models;

/// <summary>Входные данные для компонента GanttChart.</summary>
public sealed record GanttChartInput
{
    /// <summary>Дерево узлов проекта (этапы, задачи, вехи).</summary>
    public IReadOnlyList<GanttNode> Nodes { get; init; } = [];

    /// <summary>Предвычисленный календарь дней с атрибутами.</summary>
    public IReadOnlyList<CalendarDay> CalendarDays { get; init; } = [];

    /// <summary>Начальная дата отображаемого периода.</summary>
    public DateOnly StartDate { get; init; }

    /// <summary>Конечная дата отображаемого периода (включительно).</summary>
    public DateOnly EndDate { get; init; }

    /// <summary>Дата «сегодня» (для отметки на шкале).</summary>
    public DateOnly Today { get; init; }

    /// <summary>Ширина одной дневной ячейки в пикселях (по умолчанию 30).</summary>
    public int DayCellWidth { get; init; } = 30;

    /// <summary>Связи между узлами (опционально).</summary>
    public IReadOnlyList<GanttDependency> Dependencies { get; init; } = [];

    /// <summary>Запас в неделях после максимальной даты узлов (для автокоррекции шкалы, по умолчанию 2).</summary>
    public int EndPaddingWeeks { get; init; } = 2;

    /// <summary>Периоды юридического запрета (отображаются как зоны на шкале).</summary>
    public IReadOnlyList<GanttProhibition> Prohibitions { get; init; } = [];

    /// <summary>Юридические окна возможностей (отображаются как фон под задачами).</summary>
    public IReadOnlyList<GanttOpportunityWindow> OpportunityWindows { get; init; } = [];
}