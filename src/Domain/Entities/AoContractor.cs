using SamorodinkaTech.Fiducia.Domain.Enums;

namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Договор АО с регистратором или информационным агентством (ao_contractors).
/// Связывает текущее юридическое лицо (АО) с контрагентом по ИНН,
/// хранит реквизиты договора, прикреплённый файл и специфичные для
/// регистратора параметры (сроки подготовки реестров, правила ведения).
/// </summary>
public class AoContractor
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Внешний ключ на юридическое лицо — АО (legal_entity_id).</summary>
    public Guid LegalEntityId { get; set; }

    /// <summary>Навигационное свойство — юридическое лицо.</summary>
    public LegalEntity LegalEntity { get; set; } = default!;

    /// <summary>ИНН контрагента — регистратора или информационного агентства (contractor_inn).</summary>
    public string ContractorInn { get; set; } = default!;

    /// <summary>Наименование контрагента для отображения (contractor_name).</summary>
    public string ContractorName { get; set; } = default!;

    /// <summary>Тип контрагента: REGISTRAR или INFO_AGENCY (contractor_type).</summary>
    public AoContractorType ContractorType { get; set; }

    // ── Реквизиты договора ──

    /// <summary>Номер договора (contract_number).</summary>
    public string? ContractNumber { get; set; }

    /// <summary>Дата договора (contract_date).</summary>
    public DateOnly? ContractDate { get; set; }

    /// <summary>Начало действия договора (contract_valid_from).</summary>
    public DateOnly? ContractValidFrom { get; set; }

    /// <summary>Окончание действия договора (contract_valid_to).</summary>
    public DateOnly? ContractValidTo { get; set; }

    /// <summary>Признак бессрочного договора (is_indefinite). true = бессрочный, false = срочный с пролонгацией.</summary>
    public bool IsIndefinite { get; set; } = true;

    // ── Прикреплённый файл договора ──

    /// <summary>Внешний ключ на прикреплённый файл договора (contract_document_id).</summary>
    public Guid? ContractDocumentId { get; set; }

    /// <summary>Навигационное свойство — файл договора.</summary>
    public FileEntry? ContractDocument { get; set; }

    // ── Сроки подготовки (только для REGISTRAR) ──

    /// <summary>Срок подготовки реестра акционеров — количество единиц (registry_preparation_days).</summary>
    public int? RegistryPreparationDays { get; set; }

    /// <summary>Единица измерения срока подготовки реестра: CALENDAR или BUSINESS (registry_preparation_unit).</summary>
    public MeasurementUnit? RegistryPreparationUnit { get; set; }

    /// <summary>Срок подготовки реестра для выплаты дивидендов — количество единиц (dividend_registry_preparation_days).</summary>
    public int? DividendRegistryPreparationDays { get; set; }

    /// <summary>Единица измерения срока подготовки дивидендного реестра: CALENDAR или BUSINESS (dividend_registry_preparation_unit).</summary>
    public MeasurementUnit? DividendRegistryPreparationUnit { get; set; }

    // ── Правила ведения реестра (только для REGISTRAR) ──

    /// <summary>Публичная веб-ссылка на Правила ведения реестра (registry_rules_url).</summary>
    public string? RegistryRulesUrl { get; set; }

    /// <summary>Внешний ключ на файл Правил ведения реестра (registry_rules_document_id).</summary>
    public Guid? RegistryRulesDocumentId { get; set; }

    /// <summary>Навигационное свойство — файл Правил ведения реестра.</summary>
    public FileEntry? RegistryRulesDocument { get; set; }

    // ── Статус ──

    /// <summary>Признак действующего договора (is_active).</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Идентификатор создавшего пользователя (created_by).</summary>
    public Guid? CreatedBy { get; set; }
}
