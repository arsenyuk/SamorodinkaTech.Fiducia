namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Справочник типов требований участников (ref_request_type).
/// </summary>
public class RefRequestType
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Код типа (code): PREEMPTIVE_LIST, NOTARIAL_OFFER и т.д.</summary>
    public string Code { get; set; } = default!;

    /// <summary>Наименование типа (name).</summary>
    public string Name { get; set; } = default!;

    /// <summary>Доступно для ООО (is_for_llc).</summary>
    public bool IsForLlc { get; set; }

    /// <summary>Доступно для НАО (is_for_njsc).</summary>
    public bool IsForNjsc { get; set; }

    /// <summary>Доступно для ПАО (is_for_pjsc).</summary>
    public bool IsForPjsc { get; set; }

    /// <summary>Требуется приложить файл (requires_file).</summary>
    public bool RequiresFile { get; set; }

    /// <summary>Рассматривается ОСУ (considered_by_osu). TRUE — требование рассматривается общим собранием.</summary>
    public bool ConsideredByOsu { get; set; }

    /// <summary>Дата и время создания (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
