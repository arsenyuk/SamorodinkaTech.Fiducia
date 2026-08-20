namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Справочник причин отказа в предоставлении документов (ref_document_refusal_reason).
/// Закрытый перечень оснований для отказа (п. 1 ст. 50 14-ФЗ).
/// </summary>
public class RefDocumentRefusalReason
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Код причины (code): PUBLICLY_AVAILABLE, REPEATED_REQUEST и т.д.</summary>
    public string Code { get; set; } = default!;

    /// <summary>Наименование причины (name).</summary>
    public string Name { get; set; } = default!;

    /// <summary>Описание причины (description).</summary>
    public string? Description { get; set; }

    /// <summary>Правовая норма (legal_basis): ст. 50 14-ФЗ и т.д.</summary>
    public string? LegalBasis { get; set; }

    /// <summary>Порядок сортировки (sort_order).</summary>
    public int SortOrder { get; set; }

    /// <summary>Дата и время создания (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
