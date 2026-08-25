namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Сотрудник (employee).
/// Связывает участника экосистемы с юридическим лицом и должностью.
/// </summary>
public class Employee
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Ссылка на участника экосистемы (ecosystem_participant_id).</summary>
    public Guid EcosystemParticipantId { get; set; }

    /// <summary>Ссылка на юридическое лицо (legal_entity_id).</summary>
    public Guid LegalEntityId { get; set; }

    /// <summary>Должность (position).</summary>
    public string Position { get; set; } = string.Empty;

    /// <summary>Дата создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Кто создал запись (created_by).</summary>
    public Guid CreatedBy { get; set; }

    public EcosystemParticipant? EcosystemParticipant { get; set; }
    public LegalEntity? LegalEntity { get; set; }
}
