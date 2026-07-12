namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Информация о сложении полномочий члена СД, связанная с учётной записью пользователя (user_board_member_resignations).
/// Обеспечивает быстрый доступ к факту и деталям сложения полномочий со стороны пользователя.
/// </summary>
public class UserBoardMemberResignation
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Ссылка на пользователя системы (user_id).</summary>
    public Guid UserId { get; set; }

    /// <summary>Ссылка на запись назначения в СД (board_member_appointment_id).</summary>
    public Guid BoardMemberAppointmentId { get; set; }

    /// <summary>Дата сложения полномочий (resigned_at).</summary>
    public DateTime ResignedAt { get; set; }

    /// <summary>Причина сложения полномочий (resignation_reason_id).</summary>
    public Guid ResignationReasonId { get; set; }

    /// <summary>Ссылка на файл выписки из РДЛ (rdl_extract_file_id). Заполняется только для DISQUALIFICATION.</summary>
    public Guid? RdlExtractFileId { get; set; }

    /// <summary>Дата создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; }

    public User? User { get; set; }
    public BoardMemberAppointment? BoardMemberAppointment { get; set; }
    public ResignationReason? ResignationReason { get; set; }
}
