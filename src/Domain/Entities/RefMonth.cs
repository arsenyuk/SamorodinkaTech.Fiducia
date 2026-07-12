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
}
