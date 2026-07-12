namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Статус назначения члена Совета директоров (ref_board_member_appointment_statuses).
/// </summary>
public class BoardMemberAppointmentStatus
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Код статуса (code).</summary>
    public string Code { get; set; } = default!;

    /// <summary>Наименование (name).</summary>
    public string Name { get; set; } = default!;
}
