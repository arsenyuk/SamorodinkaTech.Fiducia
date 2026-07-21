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

    /// <summary>Код должности (role_code).</summary>
    public string RoleCode { get; set; } = default!;

    /// <summary>Дата начала полномочий (started_at).</summary>
    public DateOnly StartedAt { get; set; }

    /// <summary>Дата завершения полномочий (ended_at).</summary>
    public DateOnly? EndedAt { get; set; }

    /// <summary>Статус назначения (status_id).</summary>
    public Guid StatusId { get; set; }

    /// <summary>Дата сложения полномочий (resigned_at). Заполняется только при статусе RESIGNED.</summary>
    public DateTime? ResignedAt { get; set; }

    /// <summary>Причина сложения полномочий (resignation_reason_id). Заполняется только при статусе RESIGNED.</summary>
    public Guid? ResignationReasonId { get; set; }

    public BoardMember? BoardMember { get; set; }
    public BoardRole? Role { get; set; }
    public BoardMemberAppointmentStatus? Status { get; set; }
    public ResignationReason? ResignationReason { get; set; }
}
