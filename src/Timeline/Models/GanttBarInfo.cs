namespace SamorodinkaTech.Fiducia.Timeline.Models;

/// <summary>Вычисленные координаты бара на временной шкале (только для чтения).</summary>
public readonly record struct GanttBarInfo
{
    /// <summary>Индекс строки (0-based) в списке видимых строк.</summary>
    public int RowIndex { get; init; }

    /// <summary>Отступ слева в пикселях от начала шкалы.</summary>
    public int LeftPx { get; init; }

    /// <summary>Ширина бара в пикселях (0 для вех).</summary>
    public int WidthPx { get; init; }

    /// <summary>Тип узла.</summary>
    public GanttNodeType NodeType { get; init; }

    /// <summary>Прогресс (0.0 – 1.0).</summary>
    public double Progress { get; init; }

    /// <summary>Является ли вехой (для выбора формы рендеринга).</summary>
    public bool IsMilestone { get; init; }
}