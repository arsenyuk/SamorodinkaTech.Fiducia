namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Связь требований участников с планом ВОСУ (vosu_demand_links).
/// Хранит список требований, рассматриваемых на ВОСУ, с маркером инициирующего.
/// </summary>
public class VosuDemandLink
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор орг-плана ВОСУ (org_intent_id).</summary>
    public Guid OrgIntentId { get; set; }

    /// <summary>Орг-план ВОСУ.</summary>
    public OrgIntent? OrgIntent { get; set; }

    /// <summary>Идентификатор требования участника (share_request_id).</summary>
    public Guid ShareRequestId { get; set; }

    /// <summary>Требование участника.</summary>
    public ShareRequest? ShareRequest { get; set; }

    /// <summary>Признак инициирующего требования (is_initiating). True = данное требование послужило причиной созыва ВОСУ.</summary>
    public bool IsInitiating { get; set; }

    /// <summary>Идентификатор создателя записи (created_by).</summary>
    public Guid CreatedBy { get; set; }

    /// <summary>Создатель записи (пользователь).</summary>
    public User? CreatedByUser { get; set; }

    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
