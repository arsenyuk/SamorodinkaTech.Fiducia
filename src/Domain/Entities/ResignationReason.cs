namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Причина сложения полномочий члена Совета директоров (ref_resignation_reasons).
/// </summary>
public class ResignationReason
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Код причины (code).</summary>
    public string Code { get; set; } = default!;

    /// <summary>Наименование (name).</summary>
    public string Name { get; set; } = default!;
}
