namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Участник экосистемы (ecosystem_participants).
/// Связывает ФЛ с ЮЛ. Содержит атрибуты ФЛ и логин (уникальный в рамках ЮЛ).
/// Ссылка на Employee и ExternalAttractedPerson идёт через эту таблицу.
/// </summary>
public class EcosystemParticipant
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Ссылка на юридическое лицо (legal_entity_id).</summary>
    public Guid LegalEntityId { get; set; }

    /// <summary>Фамилия (last_name).</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>Имя (first_name).</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Отчество (middle_name).</summary>
    public string? MiddleName { get; set; }

    /// <summary>Email (email).</summary>
    public string? Email { get; set; }

    /// <summary>Телефон (phone).</summary>
    public string? Phone { get; set; }

    /// <summary>ИНН (inn).</summary>
    public string? Inn { get; set; }

    /// <summary>Логин (login). Уникальный в рамках ЮЛ.</summary>
    public string Login { get; set; } = string.Empty;

    /// <summary>Ссылка на учётную запись (user_id).</summary>
    public Guid? UserId { get; set; }

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
