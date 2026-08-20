namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Справочник способов доступа к документам (ref_document_access_method).
/// Определяет, каким образом участник может получить доступ к документам.
/// </summary>
public class RefDocumentAccessMethod
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Код способа (code): IN_PERSON, COPIES_ISSUE и т.д.</summary>
    public string Code { get; set; } = default!;

    /// <summary>Наименование способа (name).</summary>
    public string Name { get; set; } = default!;

    /// <summary>Описание способа (description).</summary>
    public string? Description { get; set; }

    /// <summary>Срок предоставления в рабочих днях (deadline_days). NULL — разумный срок.</summary>
    public int? DeadlineDays { get; set; }

    /// <summary>Порядок сортировки (sort_order).</summary>
    public int SortOrder { get; set; }

    /// <summary>Дата и время создания (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
