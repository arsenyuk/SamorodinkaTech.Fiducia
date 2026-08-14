namespace SamorodinkaTech.Fiducia.Timeline.Models;

/// <summary>Период юридического запрета на диаграмме Ганта.</summary>
public sealed record GanttProhibition
{
    /// <summary>Уникальный идентификатор периода запрета.</summary>
    public string Id { get; init; } = default!;

    /// <summary>Название запрета (например, «Мораторий на сделки»).</summary>
    public string Label { get; init; } = default!;

    /// <summary>Дата начала запрета (включительно).</summary>
    public DateOnly StartDate { get; init; }

    /// <summary>Дата окончания запрета (включительно).</summary>
    public DateOnly EndDate { get; init; }

    /// <summary>Идентификатор узла (GanttNode.Id), к которому привязан запрет.
    /// Если null — запрет отображается на уровне всего проекта (все строки).</summary>
    public string? NodeId { get; init; }

    /// <summary>CSS-цвет зоны запрета (по умолчанию красный).</summary>
    public string Color { get; init; } = "#e53935";

    /// <summary>Прозрачность заливки (0.0–1.0, по умолчанию 0.25).</summary>
    public double Opacity { get; init; } = 0.25;
}
