namespace SamorodinkaTech.Fiducia.Timeline.Models;

/// <summary>Тип вехи в диаграмме Ганта (визуальное различие).</summary>
public enum GanttMilestoneType
{
    /// <summary>Обычная веха (▼В*).</summary>
    Regular = 0,

    /// <summary>Межэтапная веха (▼ВР*).</summary>
    PhaseGate = 1,

    /// <summary>Юридическая веха (⚡ЮВ*). Дедлайн по закону.</summary>
    Legal = 2,

    /// <summary>Контрольная веха (⚡КВ*). Дедлайн по внутреннему документу.</summary>
    Control = 3,

    /// <summary>Интеграционная веха (▼ИН*).</summary>
    Integration = 4
}
