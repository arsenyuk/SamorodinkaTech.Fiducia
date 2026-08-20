namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Запрос участника ООО в общество (share_request).
/// Единая таблица для всех типов запросов: список участников, оферта, выход, выкуп.
/// </summary>
public class ShareRequest
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор юридического лица (legal_entity_id).</summary>
    public Guid LegalEntityId { get; set; }

    /// <summary>Идентификатор участника, подавшего запрос (participant_id).</summary>
    public Guid ParticipantId { get; set; }

    /// <summary>Идентификатор типа запроса (request_type_id).</summary>
    public Guid RequestTypeId { get; set; }

    /// <summary>Тип запроса.</summary>
    public RefRequestType? RequestType { get; set; }

    /// <summary>Статус: pending / completed / rejected (status).</summary>
    public string Status { get; set; } = "pending";

    /// <summary>Специфичные данные по типу запроса в формате JSON (payload).</summary>
    public string? Payload { get; set; }

    /// <summary>Дата и время создания запроса (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Дата и время завершения запроса (completed_at).</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Идентификатор создателя запроса (created_by).</summary>
    public Guid CreatedBy { get; set; }

    /// <summary>Дата и время отзыва запроса (revoked_at).</summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>Отзыв с нотариальным заверением (revoked_by_notarized).</summary>
    public bool RevokedByNotarized { get; set; }

    /// <summary>Видимость во входящем списке для всех (visible_to_all).</summary>
    public bool VisibleToAll { get; set; }

    /// <summary>Идентификатор нотариального заверения (notarization_id).</summary>
    public Guid? NotarizationId { get; set; }

    /// <summary>Юридическое лицо.</summary>
    public LegalEntity? LegalEntity { get; set; }

    /// <summary>Участник, подавший запрос.</summary>
    public BoardParticipant? Participant { get; set; }

    /// <summary>Создатель запроса (пользователь).</summary>
    public User? Creator { get; set; }

    /// <summary>Нотариальное заверение.</summary>
    public Notarization? Notarization { get; set; }

    // ── Коллективное требование ──────────────────────────────────

    /// <summary>Признак коллективного требования (is_collective).</summary>
    public bool IsCollective { get; set; }

    /// <summary>Порог доли для коллективного требования (threshold_percent). NULL = без порога.</summary>
    public decimal? ThresholdPercent { get; set; }

    /// <summary>Суммарная доля поддержавших (total_support_percent).</summary>
    public decimal TotalSupportPercent { get; set; }

    /// <summary>Количество поддержавших (supporter_count).</summary>
    public int SupporterCount { get; set; }

    /// <summary>Статус коллективного требования: COLLECTING / THRESHOLD_REACHED / SUBMITTED_TO_CEO / ACCEPTED / REJECTED (collective_status).</summary>
    public string? CollectiveStatus { get; set; }

    /// <summary>Дата направления ГД (submitted_to_ceo_at).</summary>
    public DateTime? SubmittedToCeoAt { get; set; }

    /// <summary>Дата решения ГД (ceo_decision_at).</summary>
    public DateTime? CeoDecisionAt { get; set; }

    /// <summary>Комментарий ГД (ceo_comment).</summary>
    public string? CeoComment { get; set; }

    /// <summary>Идентификатор ГД, принявшего решение (decided_by_user_id).</summary>
    public Guid? DecidedByUserId { get; set; }

    /// <summary>Пользователь, принявший решение.</summary>
    public User? DecidedByUser { get; set; }

    /// <summary>Поддержки участников.</summary>
    public ICollection<ShareRequestSupport> Supports { get; set; } = new List<ShareRequestSupport>();
}
