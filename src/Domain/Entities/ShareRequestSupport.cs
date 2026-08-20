namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Поддержка коллективного требования участником (share_request_support).
/// Хранит информацию о каждом участнике, поддержавшем требование.
/// </summary>
public class ShareRequestSupport
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор запроса (share_request_id).</summary>
    public Guid ShareRequestId { get; set; }

    /// <summary>Идентификатор участника, поддержавшего требование (participant_id).</summary>
    public Guid ParticipantId { get; set; }

    /// <summary>Доля участника на момент поддержки (share_percent_at_support).</summary>
    public decimal SharePercentAtSupport { get; set; }

    /// <summary>Дата и время поддержки (supported_at).</summary>
    public DateTime SupportedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Дата и время отзыва поддержки (withdrawn_at). null = активна.</summary>
    public DateTime? WithdrawnAt { get; set; }

    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Запрос участника.</summary>
    public ShareRequest? ShareRequest { get; set; }

    /// <summary>Участник.</summary>
    public BoardParticipant? Participant { get; set; }
}
