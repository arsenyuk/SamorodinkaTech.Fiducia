namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Внешнее привлеченное лицо (external_attracted_persons).
/// Параллельна таблице employee — связывает ФЛ с ЮЛ для внешних директоров/консультантов.
/// </summary>
public class ExternalAttractedPerson
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Ссылка на физическое лицо (person_id).</summary>
    public Guid PersonId { get; set; }

    /// <summary>Ссылка на юридическое лицо (legal_entity_id).</summary>
    public Guid LegalEntityId { get; set; }

    /// <summary>Должность/роль (position).</summary>
    public string Position { get; set; } = string.Empty;

    /// <summary>Дата начала привлечения (started_at).</summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>Дата окончания привлечения (ended_at).</summary>
    public DateTime? EndedAt { get; set; }

    /// <summary>Действует ли запись (is_active).</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Дата создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Кто создал запись (created_by).</summary>
    public Guid CreatedBy { get; set; }

    public Person? Person { get; set; }
    public LegalEntity? LegalEntity { get; set; }
}
