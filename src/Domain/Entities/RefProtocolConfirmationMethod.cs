namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Справочник способов подтверждения протоколов ОСУ (ref_protocol_confirmation_method).
/// </summary>
public class RefProtocolConfirmationMethod
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Код записи (code): NOTARIAL, SIGN, OTHER.</summary>
    public string Code { get; set; } = default!;

    /// <summary>Наименование для отображения в UI (name).</summary>
    public string Name { get; set; } = default!;

    /// <summary>Порядок сортировки в выпадающем списке (sort_order).</summary>
    public int SortOrder { get; set; }

    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Идентификатор создателя (created_by).</summary>
    public Guid CreatedBy { get; set; }
}
