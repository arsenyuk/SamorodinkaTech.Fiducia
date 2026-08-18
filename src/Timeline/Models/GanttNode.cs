namespace SamorodinkaTech.Fiducia.Timeline.Models;

/// <summary>Узел дерева проекта в диаграмме Ганта (этап, задача или веха).</summary>
public sealed record GanttNode
{
    /// <summary>Уникальный идентификатор узла.</summary>
    public string Id { get; init; } = default!;

    /// <summary>Идентификатор родительского узла (null для корневых этапов).</summary>
    public string? ParentId { get; init; }

    /// <summary>Название этапа, задачи или вехи.</summary>
    public string Name { get; init; } = default!;

    /// <summary>Тип узла.</summary>
    public GanttNodeType NodeType { get; init; }

    /// <summary>Дата начала (для Stage и Task).</summary>
    public DateOnly? StartDate { get; init; }

    /// <summary>Дата окончания (для Stage и Task).</summary>
    public DateOnly? EndDate { get; init; }

    /// <summary>Дата вехи (для Milestone).</summary>
    public DateOnly? MilestoneDate { get; init; }

    /// <summary>Тип вехи (для визуального различия: Regular, Legal, Control и т.д.).</summary>
    public GanttMilestoneType MilestoneType { get; init; }

    /// <summary>Прогресс выполнения (0.0 – 1.0).</summary>
    public double Progress { get; init; }

    /// <summary>Порядок сортировки среди sibling-узлов.</summary>
    public int SortOrder { get; init; }

    /// <summary>Раскрыт ли этап (для отображения дочерних элементов).</summary>
    public bool Expanded { get; set; } = true;

    /// <summary>Имя исполнителя (для тултипа).</summary>
    public string? AssignedUserName { get; init; }

    /// <summary>Путь в иерархии: «Активность > Этап > ...» (для тултипа).</summary>
    public string? BreadcrumbPath { get; init; }
}