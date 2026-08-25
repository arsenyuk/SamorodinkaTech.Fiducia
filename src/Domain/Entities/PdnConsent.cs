namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Согласие на обработку персональных данных (pdn_consents).
/// Привязано к участнику экосистемы и юридическому лицу.
/// </summary>
public class PdnConsent
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Ссылка на участника экосистемы (ecosystem_participant_id).</summary>
    public Guid EcosystemParticipantId { get; set; }

    /// <summary>Ссылка на юридическое лицо (legal_entity_id).</summary>
    public Guid LegalEntityId { get; set; }

    /// <summary>Факт выдачи согласия (consent_given).</summary>
    public bool ConsentGiven { get; set; }

    /// <summary>Дата и время выдачи согласия (consent_at).</summary>
    public DateTime? ConsentAt { get; set; }

    /// <summary>IP-адрес, с которого было выдано согласие (consent_ip).</summary>
    public string? ConsentIp { get; set; }

    /// <summary>Дата создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; }

    public EcosystemParticipant? EcosystemParticipant { get; set; }
    public LegalEntity? LegalEntity { get; set; }
}