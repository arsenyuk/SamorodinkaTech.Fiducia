namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Справочник типов документов, доступных для требования участниками (ref_document_type).
/// Определяет перечень документов, которые общество обязано предоставить по требованию.
/// </summary>
public class RefDocumentType
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Код документа (code): CHARTER, ACCOUNTING_DOCUMENTS и т.д.</summary>
    public string Code { get; set; } = default!;

    /// <summary>Наименование документа (name).</summary>
    public string Name { get; set; } = default!;

    /// <summary>Код группы (group_code): FOUNDING, PROPERTY, PROTOCOLS и т.д.</summary>
    public string GroupCode { get; set; } = default!;

    /// <summary>Наименование группы (group_name).</summary>
    public string GroupName { get; set; } = default!;

    /// <summary>Доступно в электронном виде (is_electronic_available).</summary>
    public bool IsElectronicAvailable { get; set; }

    /// <summary>Единичный документ (is_unitary). TRUE — документ уникальный, FALSE — многократный.</summary>
    public bool IsUnitary { get; set; }

    /// <summary>Срок хранения в годах (storage_years). По умолчанию 3 года.</summary>
    public int StorageYears { get; set; } = 3;

    /// <summary>Доступно для ООО (is_for_llc).</summary>
    public bool IsForLlc { get; set; }

    /// <summary>Доступно для НАО (is_for_njsc).</summary>
    public bool IsForNjsc { get; set; }

    /// <summary>Доступно для ПАО (is_for_pjsc).</summary>
    public bool IsForPjsc { get; set; }

    /// <summary>Порядок сортировки (sort_order).</summary>
    public int SortOrder { get; set; }

    /// <summary>Дата и время создания (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
