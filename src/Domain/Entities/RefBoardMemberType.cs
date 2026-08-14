namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Тип участника Совета директоров (ref_board_member_types).
/// </summary>
public class RefBoardMemberType
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Код типа (code).</summary>
    public string Code { get; set; } = default!;

    /// <summary>Наименование (name).</summary>
    public string Name { get; set; } = default!;
    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>Идентификатор создателя (created_by).</summary>
    public Guid CreatedBy { get; set; }
}
