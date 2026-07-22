namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Статус Совета директоров (ref_board_of_directors_statuses).
/// </summary>
public class BoardOfDirectorsStatus
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Код статуса (code).</summary>
    public string Code { get; set; } = default!;

    /// <summary>Наименование (name).</summary>
    public string Name { get; set; } = default!;
}
