namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Физическое лицо (person).
/// Хранит данные ФЛ и ИП. Связь с board_participant — один-к-одному (nullable FK).
/// Связь с identity_documents — один-ко-многим.
/// </summary>
public class Person
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Фамилия (last_name).</summary>
    public string LastName { get; set; } = default!;

    /// <summary>Имя (first_name).</summary>
    public string FirstName { get; set; } = default!;

    /// <summary>Отчество (middle_name).</summary>
    public string? MiddleName { get; set; }

    /// <summary>ФИО — вычисляемое свойство (Last First Middle).</summary>
    public string FullName => string.Join(" ", new[] { LastName, FirstName, MiddleName }
        .Where(x => !string.IsNullOrWhiteSpace(x)));

    /// <summary>ИНН (inn). Используется для ФЛ и ИП.</summary>
    public string? Inn { get; set; }

    /// <summary>Гражданство (citizenship).</summary>
    public string? Citizenship { get; set; }

    /// <summary>СНИЛС (snils).</summary>
    public string? Snils { get; set; }

    /// <summary>ОГРНИП (ogrnip). Заполняется для ИП.</summary>
    public string? Ogrnip { get; set; }

    /// <summary>Дата создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Дата последнего обновления (updated_at).</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Идентификатор создателя записи (created_by).</summary>
    public Guid? CreatedBy { get; set; }
}
