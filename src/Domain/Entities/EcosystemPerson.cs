namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Физическое лицо участника экосистемы (ecosystem_persons).
/// Хранит ФИО, контактные данные. Связь с ecosystem_participant — один-к-одному.
/// </summary>
public class EcosystemPerson
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Фамилия (last_name).</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>Имя (first_name).</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Отчество (middle_name).</summary>
    public string? MiddleName { get; set; }

    /// <summary>Дата создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Кто создал запись (created_by).</summary>
    public Guid? CreatedBy { get; set; }
}
