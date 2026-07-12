namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Назначение на должность члена СД (board_member_appointments, SCD Type 2).
/// </summary>
public class BoardMemberAppointment
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Ссылка на члена СД (board_member_id).</summary>
    public Guid BoardMemberId { get; set; }

    /// <summary>Ссылка на должность (role_id).</summary>
    public Guid RoleId { get; set; }

    /// <summary>Дата начала полномочий (started_at).</summary>
    public DateOnly StartedAt { get; set; }

    /// <summary>Дата завершения полномочий (ended_at).</summary>
    public DateOnly? EndedAt { get; set; }

    /// <summary>Статус назначения (status).</summary>
    public string Status { get; set; } = "ACTIVE";

    public BoardMember? BoardMember { get; set; }
    public BoardRole? Role { get; set; }
}
