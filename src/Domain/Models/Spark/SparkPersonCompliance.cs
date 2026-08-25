namespace SamorodinkaTech.Fiducia.Domain.Models.Spark;

/// <summary>
/// Отчёт о соответствии (санкционные риски) из СПАРК — SOAP-метод GetPersonComplianceReport.
/// Проверка на санкции, связи с ПДЛ, включении в реестры.
/// </summary>
public class SparkPersonCompliance
{
    /// <summary>ФИО проверяемого лица.</summary>
    public string FullName { get; init; } = default!;

    /// <summary>ИНН проверяемого лица.</summary>
    public string? Inn { get; init; }

    /// <summary>Общий уровень риска: low/medium/high/critical.</summary>
    public string RiskLevel { get; init; } = "unknown";

    /// <summary>Признак включения в санкционные списки.</summary>
    public bool IsSanctioned { get; init; }

    /// <summary>Признак связи с ПДЛ (публичные должностные лица).</summary>
    public bool IsPdl { get; init; }

    /// <summary>Признак связи с ПЭП (политически значимые лица).</summary>
    public bool IsPep { get; init; }

    /// <summary>Список санкций (страна + тип).</summary>
    public List<SparkSanctionEntry> Sanctions { get; init; } = new();

    /// <summary>Связи с ПДЛ/ПЭП.</summary>
    public List<SparkPdlRelation> PdlRelations { get; init; } = new();

    /// <summary>Включение в реестры (تخاذшие меры противодействия).</summary>
    public List<SparkRegistryEntry> Registries { get; init; } = new();

    /// <summary>Дата проверки.</summary>
    public DateTime? CheckDate { get; init; }

    /// <summary>Источник данных.</summary>
    public string? DataSource { get; init; }

    /// <summary>Дополнительная информация.</summary>
    public string? AdditionalInfo { get; init; }
}

/// <summary>
/// Запись о санкции.
/// </summary>
public class SparkSanctionEntry
{
    /// <summary>Страна, ввёшая санкции.</summary>
    public string? Country { get; init; }

    /// <summary>Тип санкций.</summary>
    public string? SanctionType { get; init; }

    /// <summary>Основание.</summary>
    public string? Basis { get; init; }

    /// <summary>Дата введения.</summary>
    public DateTime? DateImposed { get; init; }

    /// <summary>Дата снятия (если сняты).</summary>
    public DateTime? DateRemoved { get; init; }
}

/// <summary>
/// Связь с ПДЛ/ПЭП.
/// </summary>
public class SparkPdlRelation
{
    /// <summary>ФИО связанного лица.</summary>
    public string FullName { get; init; } = default!;

    /// <summary>ИНН связанного лица.</summary>
    public string? Inn { get; init; }

    /// <summary>Тип связи: родственник, деловой партнёр, совладелец и т.д.</summary>
    public string? RelationType { get; init; }

    /// <summary>Должность (если ПДЛ).</summary>
    public string? Position { get; init; }

    /// <summary>Организация.</summary>
    public string? Organization { get; init; }
}

/// <summary>
/// Запись о включении в реестр.
/// </summary>
public class SparkRegistryEntry
{
    /// <summary>Наименование реестра.</summary>
    public string? RegistryName { get; init; }

    /// <summary>Страна реестра.</summary>
    public string? Country { get; init; }

    /// <summary>Дата включения.</summary>
    public DateTime? DateAdded { get; init; }

    /// <summary>Дата исключения.</summary>
    public DateTime? DateRemoved { get; init; }
}
