namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Роль участника в Совете директоров (board_participant_role).
/// Junction-таблица: какой участник какую роль занимает в СД.
/// </summary>
public class BoardParticipantRole
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор участника общества (participant_id).</summary>
    public Guid ParticipantId { get; set; }

    /// <summary>Участник общества.</summary>
    public BoardParticipant? Participant { get; set; }

    /// <summary>Идентификатор роли в СД (role_id).</summary>
    public Guid RoleId { get; set; }

    /// <summary>Роль в СД.</summary>
    public RefBoardRole? Role { get; set; }

    /// <summary>Дата и время назначения (assigned_at).</summary>
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Идентификатор того, кто назначил (assigned_by).</summary>
    public Guid? AssignedBy { get; set; }
}
