namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Соглашение о ПЭП (pep_agreements).
/// Привязано к участнику экосистемы.
/// </summary>
public class PepAgreement
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Ссылка на участника экосистемы (ecosystem_participant_id).</summary>
    public Guid EcosystemParticipantId { get; set; }

    /// <summary>Факт подписания соглашения (agreement_signed).</summary>
    public bool AgreementSigned { get; set; }

    /// <summary>Дата и время подписания (signed_at).</summary>
    public DateTime? SignedAt { get; set; }

    /// <summary>Дата создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; }

    public EcosystemParticipant? EcosystemParticipant { get; set; }
}