namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Справочник ОКОПФ (ref_okopf). UUID PK + код и наименование.
/// </summary>
public class RefOkopf
{
    public Guid Id { get; set; }
    public string Code { get; set; } = default!; // Например: 12247
    public string Name { get; set; } = default!; // Полное наименование
    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>Идентификатор создателя (created_by).</summary>
    public Guid CreatedBy { get; set; }
}
