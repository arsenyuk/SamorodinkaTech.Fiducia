namespace SamorodinkaTech.Fiducia.Timeline.Models;

/// <summary>Юридическое окно возможностей на диаграмме Ганта.</summary>
public sealed record GanttOpportunityWindow
{
    /// <summary>Уникальный идентификатор окна.</summary>
    public string Id { get; init; } = default!;

    /// <summary>Название окна (например, «Окно для собрания СД»).</summary>
    public string Label { get; init; } = default!;

    /// <summary>Дата начала окна (включительно).</summary>
    public DateOnly StartDate { get; init; }

    /// <summary>Дата окончания окна (включительно).</summary>
    public DateOnly EndDate { get; init; }

    /// <summary>Идентификатор узла (GanttNode.Id), к которому привязано окно.
    /// Если null — окно отображается на уровне всего проекта (все строки).</summary>
    public string? NodeId { get; init; }

    /// <summary>CSS-цвет зоны окна (по умолчанию сине-зелёный).</summary>
    public string Color { get; init; } = "#00897b";

    /// <summary>Прозрачность заливки (0.0–1.0, по умолчанию 0.2).</summary>
    public double Opacity { get; init; } = 0.2;
}
