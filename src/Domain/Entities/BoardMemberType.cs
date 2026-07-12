namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Тип участника Совета директоров (ref_board_member_types).
/// </summary>
public class BoardMemberType
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Код типа (code).</summary>
    public string Code { get; set; } = default!;

    /// <summary>Наименование (name).</summary>
    public string Name { get; set; } = default!;
}
