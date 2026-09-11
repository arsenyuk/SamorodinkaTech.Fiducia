namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Участник экосистемы (ecosystem_participants).
/// Связывает ФЛ с ЮЛ. Ссылка на Employee и ExternalAttractedPerson идёт через эту таблицу.
/// </summary>
public class EcosystemParticipant
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Ссылка на юридическое лицо (legal_entity_id).</summary>
    public Guid LegalEntityId { get; set; }

    /// <summary>Ссылка на ФЛ участника экосистемы (ecosystem_person_id).</summary>
    public Guid EcosystemPersonId { get; set; }

    /// <summary>Физическое лицо участника экосистемы.</summary>
    public EcosystemPerson? EcosystemPerson { get; set; }

    /// <summary>Ссылка на учётную запись (user_id).</summary>
    public Guid? UserId { get; set; }

    /// <summary>Признак активности участника экосистемы (is_active).</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Дата создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Кто создал запись (created_by).</summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>Навигация: учётная запись пользователя.</summary>
    public User? User { get; set; }

    /// <summary>Навигация: юридическое лицо.</summary>
    public LegalEntity? LegalEntity { get; set; }

    /// <summary>Согласия на обработку ПДн.</summary>
    public ICollection<PdnConsent> PdnConsents { get; set; } = new List<PdnConsent>();

    /// <summary>Соглашения о ПЭП.</summary>
    public ICollection<PepAgreement> PepAgreements { get; set; } = new List<PepAgreement>();

    /// <summary>Анкеты независимости.</summary>
    public ICollection<IndependenceDeclaration> IndependenceDeclarations { get; set; } = new List<IndependenceDeclaration>();
}
