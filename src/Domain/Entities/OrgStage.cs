namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Этап организационного мероприятия (org_stages).
/// Второй уровень иерархии, привязан к OrgIntent.
/// </summary>
public class OrgStage
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор цели (intent_id).</summary>
    public Guid IntentId { get; set; }

    /// <summary>Цель.</summary>
    public OrgIntent? Intent { get; set; }

    /// <summary>Наименование этапа (name).</summary>
    public string Name { get; set; } = default!;

    /// <summary>Описание (description).</summary>
    public string? Description { get; set; }

    /// <summary>Порядок сортировки (sort_order).</summary>
    public int SortOrder { get; set; }

    /// <summary>Смещение начала относительно родителя, дни (start_offset_days).</summary>
    public int? StartOffsetDays { get; set; }

    /// <summary>Принцип вычисления дедлайна: FIXED_DAYS, BEFORE_DATE, AFTER_START (deadline_rule).</summary>
    public string? DeadlineRule { get; set; }

    /// <summary>Количество дней до дедлайна (deadline_days).</summary>
    public int? DeadlineDays { get; set; }

    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Оферы, относящиеся к данному этапу.</summary>
    public ICollection<OrgOffer>? Offers { get; set; }
}
