namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Должность в Совете директоров (ref_board_roles).
/// </summary>
public class RefBoardRole
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Код должности (code).</summary>
    public string Code { get; set; } = default!;

    /// <summary>Наименование (name).</summary>
    public string Name { get; set; } = default!;

    /// <summary>Порядок сортировки (sort_order).</summary>
    public int SortOrder { get; set; }
    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>Идентификатор создателя (created_by).</summary>
    public Guid CreatedBy { get; set; }
}
