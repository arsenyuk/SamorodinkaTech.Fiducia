namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Цель организационного мероприятия (tpl_org_intents).
/// Верхний уровень иерархии шаблонов: Intent → Stage → Offer → Task.
/// </summary>
public class TplOrgIntent
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Код цели для программного поиска (code).</summary>
    public string? Code { get; set; }

    /// <summary>Наименование цели (name).</summary>
    public string Name { get; set; } = default!;

    /// <summary>Описание (description).</summary>
    public string? Description { get; set; }

    /// <summary>Порядок сортировки (sort_order).</summary>
    public int SortOrder { get; set; }

    /// <summary>Применим для АО — ПАО/НАО/АО (is_for_ao). null — не указано.</summary>
    public bool? IsForAo { get; set; }

    /// <summary>Применим для ООО (is_for_llc). null — не указано.</summary>
    public bool? IsForLlc { get; set; }

    /// <summary>Требует наличия Совета директоров (requires_board_of_directors). null — не указано.</summary>
    public bool? RequiresBoardOfDirectors { get; set; }

    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Этапы, относящиеся к данной цели.</summary>
    public ICollection<TplOrgStage>? Stages { get; set; }
}
