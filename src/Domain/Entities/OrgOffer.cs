namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Офер (задача с пулом кандидатов) организационного мероприятия (org_offers).
/// Третий уровень иерархии, привязан к OrgStage.
/// </summary>
public class OrgOffer
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор этапа (stage_id).</summary>
    public Guid StageId { get; set; }

    /// <summary>Этап.</summary>
    public OrgStage? Stage { get; set; }

    /// <summary>Наименование офера (name).</summary>
    public string Name { get; set; } = default!;

    /// <summary>Описание (description).</summary>
    public string? Description { get; set; }

    /// <summary>Порядок сортировки (sort_order).</summary>
    public int SortOrder { get; set; }

    /// <summary>Смещение начала относительно родителя, дни (start_offset_days).</summary>
    public int? StartOffsetDays { get; set; }

    /// <summary>Принцип вычисления дедлайна (deadline_rule).</summary>
    public string? DeadlineRule { get; set; }

    /// <summary>Количество дней до дедлайна (deadline_days).</summary>
    public int? DeadlineDays { get; set; }

    /// <summary>JSON-массив role_id кандидатов (candidate_roles).</summary>
    public string? CandidateRoles { get; set; }

    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Задачи, относящиеся к данному оферу.</summary>
    public ICollection<OrgTask>? Tasks { get; set; }
}
