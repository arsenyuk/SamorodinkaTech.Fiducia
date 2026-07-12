namespace SamorodinkaTech.Fiducia.Timeline.Models;

/// <summary>Связь между двумя узлами диаграммы Ганта.</summary>
public sealed record GanttDependency
{
    /// <summary>Идентификатор узла-предшественника.</summary>
    public string FromId { get; init; } = default!;

    /// <summary>Идентификатор узла-последователя.</summary>
    public string ToId { get; init; } = default!;

    /// <summary>Тип связи.</summary>
    public GanttDependencyType Type { get; init; } = GanttDependencyType.Regular;

    /// <summary>Цвет линии (CSS-совместимый, например #ff0000).</summary>
    public string Color { get; init; } = "#90a4ae";

    /// <summary>Стиль линии: solid или dashed.</summary>
    public string Style { get; init; } = "solid";

    /// <summary>Толщина линии в пикселях.</summary>
    public int Thickness { get; init; } = 1;
}