namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Повестка будущего заседания СД или ВОСА (agenda_items).
/// Создаётся автоматически при событиях, требующих созыва заседания
/// (сложение полномочий, потеря кворума, выбытие председателя и др.).
/// </summary>
public class AgendaItem
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Ссылка на Совет директоров (board_of_directors_id).</summary>
    public Guid BoardOfDirectorsId { get; set; }

    /// <summary>Ссылка на юридическое лицо (legal_entity_id).</summary>
    public Guid? LegalEntityId { get; set; }

    /// <summary>Наименование пункта повестки (title).</summary>
    public string Title { get; set; } = default!;

    /// <summary>Тип целевого заседания (target_type): BOARD_MEETING или OSA.</summary>
    public string TargetType { get; set; } = default!;

    /// <summary>Причина создания повестки (reason).</summary>
    public string Reason { get; set; } = default!;

    /// <summary>Статус (status): PENDING.</summary>
    public string Status { get; set; } = "PENDING";

    /// <summary>Дата и время создания (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
