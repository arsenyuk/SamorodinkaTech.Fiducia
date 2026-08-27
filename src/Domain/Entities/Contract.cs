using SamorodinkaTech.Fiducia.Domain.Enums;

namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Единая сущность договора (contracts).
/// Поддерживает типы: REGISTRAR, INFO_AGENCY, MANAGEMENT_IP, MANAGEMENT_UL.
/// </summary>
public class Contract
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Внешний ключ на юридическое лицо (legal_entity_id).</summary>
    public Guid LegalEntityId { get; set; }

    /// <summary>Навигационное свойство — юридическое лицо.</summary>
    public LegalEntity LegalEntity { get; set; } = default!;

    /// <summary>Тип договора (contract_type).</summary>
    public ContractType ContractType { get; set; }

    // ── Данные контрагента (общие) ──

    /// <summary>Наименование контрагента (counterparty_name).</summary>
    public string CounterpartyName { get; set; } = default!;

    /// <summary>ИНН контрагента (counterparty_inn).</summary>
    public string CounterpartyInn { get; set; } = default!;

    // ── Реквизиты договора (общие) ──

    /// <summary>Номер договора (contract_number).</summary>
    public string? ContractNumber { get; set; }

    /// <summary>Дата договора (contract_date).</summary>
    public DateOnly? ContractDate { get; set; }

    /// <summary>Начало действия договора (contract_valid_from).</summary>
    public DateOnly? ContractValidFrom { get; set; }

    /// <summary>Окончание действия договора (contract_valid_to).</summary>
    public DateOnly? ContractValidTo { get; set; }

    /// <summary>Признак бессрочного договора (is_indefinite).</summary>
    public bool IsIndefinite { get; set; } = true;

    /// <summary>Внешний ключ на прикреплённый файл договора (contract_document_id).</summary>
    public Guid? ContractDocumentId { get; set; }

    /// <summary>Навигационное свойство — файл договора.</summary>
    public FileEntry? ContractDocument { get; set; }

    // ── Сроки подготовки реестров (REGISTRAR) ──

    /// <summary>Срок подготовки реестра акционеров — количество единиц (registry_preparation_days).</summary>
    public int? RegistryPreparationDays { get; set; }

    /// <summary>Единица измерения срока подготовки реестра (registry_preparation_unit).</summary>
    public Guid? RegistryPreparationUnitId { get; set; }

    /// <summary>Справочник единиц измерения.</summary>
    public RefMeasurementUnit? RegistryPreparationUnit { get; set; }

    /// <summary>Срок подготовки реестра для дивидендов — количество единиц (dividend_registry_preparation_days).</summary>
    public int? DividendRegistryPreparationDays { get; set; }

    /// <summary>Единица измерения срока подготовки дивидендного реестра (dividend_registry_preparation_unit).</summary>
    public Guid? DividendRegistryPreparationUnitId { get; set; }

    /// <summary>Справочник единиц измерения.</summary>
    public RefMeasurementUnit? DividendRegistryPreparationUnit { get; set; }

    // ── Правила ведения реестра (REGISTRAR) ──

    /// <summary>Публичная веб-ссылка на Правила ведения реестра (registry_rules_url).</summary>
    public string? RegistryRulesUrl { get; set; }

    /// <summary>Внешний ключ на файл Правил ведения реестра (registry_rules_document_id).</summary>
    public Guid? RegistryRulesDocumentId { get; set; }

    /// <summary>Навигационное свойство — файл Правил ведения реестра.</summary>
    public FileEntry? RegistryRulesDocument { get; set; }

    // ── Управляющий ИП (MANAGEMENT_IP) ──

    /// <summary>ОГРНИП ИП-управляющего (manager_ogrnip) — 15 цифр.</summary>
    public string? ManagerOgrnip { get; set; }

    // ── Управляющий ЮЛ (MANAGEMENT_UL) ──

    /// <summary>Внешний ключ на ЮЛ-управляющего из справочника (manager_legal_entity_id).</summary>
    public Guid? ManagerLegalEntityId { get; set; }

    /// <summary>Навигационное свойство — ЮЛ-управляющий.</summary>
    public LegalEntity? ManagerLegalEntity { get; set; }

    // ── Статус ──

    /// <summary>Признак действующего договора (is_active).</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Идентификатор создавшего пользователя (created_by).</summary>
    public Guid? CreatedBy { get; set; }
}
