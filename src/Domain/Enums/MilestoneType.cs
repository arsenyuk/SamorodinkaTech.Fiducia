namespace SamorodinkaTech.Fiducia.Domain.Enums;

/// <summary>
/// Тип вехи в диаграмме Ганта.
/// </summary>
public enum MilestoneType
{
    /// <summary>Обычная веха (▼В*). Внутрифазовая контрольная точка.</summary>
    REGULAR = 0,

    /// <summary>Межэтапная веха (▼ВР*). Разделитель между фазами.</summary>
    PHASE_GATE = 1,

    /// <summary>Юридическая веха (⚡ЮВ*). Дедлайн, установленный законодательством.</summary>
    LEGAL = 2,

    /// <summary>Контрольная веха (⚡КВ*). Дедлайн, установленный внутренним документом.</summary>
    CONTROL = 3,

    /// <summary>Интеграционная веха (▼ИН*). Точка синхронизации двух+ критических путей.</summary>
    INTEGRATION = 4
}
