namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Причина сложения полномочий члена Совета директоров (ref_resignation_reasons).
/// </summary>
public class RefResignationReason
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Код причины (code).</summary>
    public string Code { get; set; } = default!;

    /// <summary>Наименование (name).</summary>
    public string Name { get; set; } = default!;
    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>Идентификатор создателя (created_by).</summary>
    public Guid CreatedBy { get; set; }
}
