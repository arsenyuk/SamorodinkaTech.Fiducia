namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Шаблон уведомления (notification_template).
/// Хранит шаблоны Title и Body для каждого типа уведомления.
/// Плейсхолдеры подставляются при формировании уведомления.
/// </summary>
public class NotificationTemplate
{
    /// <summary>Уникальный идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Код типа уведомления (notification_type_code). Ссылка на ref_notification_type.code.</summary>
    public string NotificationTypeCode { get; set; } = string.Empty;

    /// <summary>Шаблон заголовка (title_template). Содержит плейсхолдеры {name}.</summary>
    public string TitleTemplate { get; set; } = string.Empty;

    /// <summary>Шаблон тела письма (body_template). Содержит плейсхолдеры {name}.</summary>
    public string BodyTemplate { get; set; } = string.Empty;

    /// <summary>Описание шаблона для администратора (description).</summary>
    public string? Description { get; set; }

    /// <summary>Включён ли шаблон (is_enabled). Выключенные шаблоны используют fallback.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Дата последнего обновления (updated_at).</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
