namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Справочник ОКОПФ (ref_okopf). UUID PK + код и наименование.
/// </summary>
public class Okopf
{
    public Guid Id { get; set; }
    public string Code { get; set; } = default!; // Например: 12247
    public string Name { get; set; } = default!; // Полное наименование
}
