namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Системная настройка (system_settings). Хранит пары ключ-значение.
/// </summary>
public class SystemSetting
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Ключ настройки (key).</summary>
    public string Key { get; set; } = default!;

    /// <summary>Значение настройки (value).</summary>
    public string? Value { get; set; }

    /// <summary>Описание (description).</summary>
    public string? Description { get; set; }

    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Дата и время обновления записи (updated_at).</summary>
    public DateTime? UpdatedAt { get; set; }
}
