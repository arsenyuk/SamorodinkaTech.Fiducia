namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Цель организационного мероприятия (org_intents).
/// Верхний уровень иерархии шаблонов: Intent → Stage → Offer → Task.
/// </summary>
public class OrgIntent
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Наименование цели (name).</summary>
    public string Name { get; set; } = default!;

    /// <summary>Описание (description).</summary>
    public string? Description { get; set; }

    /// <summary>Порядок сортировки (sort_order).</summary>
    public int SortOrder { get; set; }

    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Этапы, относящиеся к данной цели.</summary>
    public ICollection<OrgStage>? Stages { get; set; }
}
