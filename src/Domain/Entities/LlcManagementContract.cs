namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Договор ООО с управляющим ИП (llc_management_contracts).
/// Ст. 42 14-ФЗ: общество вправе передать по договору полномочия ЕИО управляющему.
/// </summary>
public class LlcManagementContract
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор юридического лица (legal_entity_id).</summary>
    public Guid LegalEntityId { get; set; }

    /// <summary>Юридическое лицо.</summary>
    public LegalEntity? LegalEntity { get; set; }

    /// <summary>ФИО ИП-управляющего (manager_full_name).</summary>
    public string ManagerFullName { get; set; } = default!;

    /// <summary>ИНН ИП (manager_inn) — 12 цифр.</summary>
    public string ManagerInn { get; set; } = default!;

    /// <summary>ОГРНИП (manager_ogrnip) — 15 цифр.</summary>
    public string? ManagerOgrnip { get; set; }

    /// <summary>Номер договора (contract_number).</summary>
    public string? ContractNumber { get; set; }

    /// <summary>Дата договора (contract_date).</summary>
    public DateOnly? ContractDate { get; set; }

    /// <summary>Начало действия договора (contract_valid_from).</summary>
    public DateOnly ContractValidFrom { get; set; }

    /// <summary>Окончание действия договора (contract_valid_to). NULL = бессрочный.</summary>
    public DateOnly? ContractValidTo { get; set; }

    /// <summary>Признак бессрочного договора (is_indefinite).</summary>
    public bool IsIndefinite { get; set; } = true;

    /// <summary>Внешний ключ на прикреплённый файл договора (contract_document_id).</summary>
    public Guid? ContractDocumentId { get; set; }

    /// <summary>Файл договора.</summary>
    public FileEntry? ContractDocument { get; set; }

    /// <summary>Действующий договор (is_active).</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Идентификатор создателя (created_by).</summary>
    public Guid? CreatedBy { get; set; }
}
