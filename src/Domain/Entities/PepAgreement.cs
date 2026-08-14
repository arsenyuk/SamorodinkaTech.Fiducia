namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Соглашение о ПЭП (pep_agreements).
/// Привязано к физическому лицу (persons).
/// </summary>
public class PepAgreement
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Ссылка на физическое лицо (person_id).</summary>
    public Guid PersonId { get; set; }

    /// <summary>Факт подписания соглашения (agreement_signed).</summary>
    public bool AgreementSigned { get; set; }

    /// <summary>Дата и время подписания (signed_at).</summary>
    public DateTime? SignedAt { get; set; }

    /// <summary>Дата создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; }

    public Person? Person { get; set; }
}