namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Сотрудник (employee).
/// Связывает физическое лицо с юридическим лицом и должностью.
/// </summary>
public class Employee
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Ссылка на физическое лицо (person_id).</summary>
    public Guid PersonId { get; set; }

    /// <summary>Ссылка на юридическое лицо (legal_entity_id).</summary>
    public Guid LegalEntityId { get; set; }

    /// <summary>Должность (position).</summary>
    public string Position { get; set; } = string.Empty;

    /// <summary>Дата создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Кто создал запись (created_by).</summary>
    public Guid CreatedBy { get; set; }

    public Person? Person { get; set; }
    public LegalEntity? LegalEntity { get; set; }
}
