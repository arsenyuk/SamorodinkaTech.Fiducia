namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Уведомление участнику ВОСУ (vosu_notifications).
/// Ссылается на конкретного участника (BoardParticipant) и сформированный DOCX-файл.
/// </summary>
public class VosuNotification
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор орг-плана ВОСУ (org_intent_id).</summary>
    public Guid OrgIntentId { get; set; }

    /// <summary>Орг-план ВОСУ.</summary>
    public OrgIntent? OrgIntent { get; set; }

    /// <summary>Идентификатор участника СД (board_participant_id).</summary>
    public Guid BoardParticipantId { get; set; }

    /// <summary>Участник СД.</summary>
    public BoardParticipant? BoardParticipant { get; set; }

    /// <summary>Идентификатор DOCX-файла (file_id).</summary>
    public Guid FileId { get; set; }

    /// <summary>DOCX-файл уведомления.</summary>
    public FileEntry? File { get; set; }

    /// <summary>Идентификатор создателя записи (created_by).</summary>
    public Guid CreatedBy { get; set; }

    /// <summary>Создатель записи (пользователь).</summary>
    public User? CreatedByUser { get; set; }

    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
