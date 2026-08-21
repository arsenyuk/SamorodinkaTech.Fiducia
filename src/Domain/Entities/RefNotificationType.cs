namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Справочник типов уведомлений (ref_notification_type).
/// Хранит коды и наименования типов уведомлений.
/// </summary>
public class RefNotificationType
{
    /// <summary>Уникальный идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Код типа уведомления (code). Уникальный, UPPER_SNAKE_CASE.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Наименование типа уведомления (name).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Идентификатор создателя записи (created_by).</summary>
    public Guid CreatedBy { get; set; }
}
