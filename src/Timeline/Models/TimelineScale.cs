namespace SamorodinkaTech.Fiducia.Timeline.Models;

/// <summary>Масштаб (уровень детализации) нижнего ряда шкалы.</summary>
public enum TimelineScale
{
    /// <summary>Дни.</summary>
    Days,

    /// <summary>Недели (пн–вс).</summary>
    Weeks,

    /// <summary>Месяцы.</summary>
    Months,

    /// <summary>Кварталы.</summary>
    Quarters,

    /// <summary>Годы.</summary>
    Years
}