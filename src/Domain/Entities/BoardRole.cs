namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Должность в Совете директоров (ref_board_roles).
/// </summary>
public class BoardRole
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Код должности (code).</summary>
    public string Code { get; set; } = default!;

    /// <summary>Наименование (name).</summary>
    public string Name { get; set; } = default!;

    /// <summary>Порядок сортировки (sort_order).</summary>
    public int SortOrder { get; set; }
}
