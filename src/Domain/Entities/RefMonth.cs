namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Справочник месяцев (ref_month).
/// </summary>
public class RefMonth
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Номер месяца (code).</summary>
    public string Code { get; set; } = default!;

    /// <summary>Наименование (name).</summary>
    public string Name { get; set; } = default!;
    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>Идентификатор создателя (created_by).</summary>
    public Guid CreatedBy { get; set; }
}
