namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Настройки доступности документов для ЮЛ (legal_entity_document_access).
/// Определяет, какие типы документов могут предоставляться в электронном виде для конкретного ЮЛ.
/// </summary>
public class LegalEntityDocumentAccess
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор юридического лица (legal_entity_id).</summary>
    public Guid LegalEntityId { get; set; }

    /// <summary>Код типа документа (document_type_code) — FK на ref_document_type.code.</summary>
    public string DocumentTypeCode { get; set; } = default!;

    /// <summary>Доступно в электронном виде (is_electronic_available).</summary>
    public bool IsElectronicAvailable { get; set; }

    /// <summary>Дата и время создания (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Юридическое лицо (навигация).</summary>
    public LegalEntity? LegalEntity { get; set; }
}
