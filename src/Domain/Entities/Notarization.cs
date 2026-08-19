namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Нотариальное заверение (notarization).
/// Единая таблица для всех нотариальных действий в системе.
/// Хранит скан документа + атрибуты заверения.
/// </summary>
public class Notarization
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор юридического лица (legal_entity_id).</summary>
    public Guid LegalEntityId { get; set; }

    /// <summary>Тип документа: SHARE_OFFER / EXIT_APPLICATION / OTHER (document_type).</summary>
    public string DocumentType { get; set; } = default!;

    /// <summary>Идентификатор связанной сущности (related_entity_id).</summary>
    public Guid? RelatedEntityId { get; set; }

    /// <summary>Тип связанной сущности (related_entity_type).</summary>
    public string? RelatedEntityType { get; set; }

    /// <summary>Идентификатор скана документа (document_file_id).</summary>
    public Guid DocumentFileId { get; set; }

    /// <summary>ФИО нотариуса (notary_full_name).</summary>
    public string NotaryFullName { get; set; } = default!;

    /// <summary>Номер лицензии нотариуса (notary_license_number).</summary>
    public string? NotaryLicenseNumber { get; set; }

    /// <summary>Реестровый номер в ЕИС (registry_number).</summary>
    public string? RegistryNumber { get; set; }

    /// <summary>Дата заверения (notarization_date).</summary>
    public DateOnly NotarizationDate { get; set; }

    /// <summary>Действует с (valid_from).</summary>
    public DateOnly? ValidFrom { get; set; }

    /// <summary>Действует до (valid_until). NULL = бессрочно.</summary>
    public DateOnly? ValidUntil { get; set; }

    /// <summary>Время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Идентификатор создателя (created_by).</summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>Скан документа.</summary>
    public FileEntry? DocumentFile { get; set; }

    /// <summary>Юридическое лицо.</summary>
    public LegalEntity? LegalEntity { get; set; }
}
