namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Дополнительные настройки юридического лица (legal_entity_extra_settings).
/// 1:1 связь с LegalEntity.
/// </summary>
public class LegalEntityExtraSettings
{
    /// <summary>Идентификатор юридического лица — первичный ключ (legal_entity_id).</summary>
    public Guid LegalEntityId { get; set; }

    /// <summary>Юридическое лицо.</summary>
    public LegalEntity? LegalEntity { get; set; }

    /// <summary>Ведение списка участников через нотариат утверждено (notary_list_approved).</summary>
    public bool NotaryListApproved { get; set; }

    /// <summary>Ссылка на протокол ОСА/ОСУ (notary_list_osa_meeting_id).</summary>
    public Guid? NotaryListOsaMeetingId { get; set; }

    /// <summary>Протокол ОСА/ОСУ.</summary>
    public OsaMeeting? NotaryListOsaMeeting { get; set; }

    /// <summary>Дата решения (notary_list_decision_date).</summary>
    public DateOnly? NotaryListDecisionDate { get; set; }
}
