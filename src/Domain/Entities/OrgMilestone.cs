using SamorodinkaTech.Fiducia.Domain.Enums;

namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Реальная веха организационного мероприятия (org_milestones), созданная из шаблона tpl_org_milestones.
/// </summary>
public class OrgMilestone
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор цели (intent_id).</summary>
    public Guid IntentId { get; set; }

    /// <summary>Цель.</summary>
    public OrgIntent? Intent { get; set; }

    /// <summary>Идентификатор шаблона (template_milestone_id).</summary>
    public Guid? TemplateMilestoneId { get; set; }

    /// <summary>Шаблон.</summary>
    public TplOrgMilestone? TemplateMilestone { get; set; }

    /// <summary>Идентификатор этапа (stage_id).</summary>
    public Guid? StageId { get; set; }

    /// <summary>Этап.</summary>
    public OrgStage? Stage { get; set; }

    /// <summary>Наименование вехи (name).</summary>
    public string Name { get; set; } = default!;

    /// <summary>Описание (description).</summary>
    public string? Description { get; set; }

    /// <summary>Тип вехи (milestone_type).</summary>
    public MilestoneType MilestoneType { get; set; }

    /// <summary>JSON-массив ID задач-предшественников (predecessor_task_ids).</summary>
    public string? PredecessorTaskIds { get; set; }

    /// <summary>JSON-массив ID этапов-предшественников (predecessor_stage_ids).</summary>
    public string? PredecessorStageIds { get; set; }

    /// <summary>Плановая дата наступления (planned_date).</summary>
    public DateOnly? PlannedDate { get; set; }

    /// <summary>Фактическая дата наступления (actual_date).</summary>
    public DateOnly? ActualDate { get; set; }

    /// <summary>Статус: PLANNED, COMPLETED, MISSED (status).</summary>
    public string Status { get; set; } = "PLANNED";

    /// <summary>Порядок сортировки (sort_order).</summary>
    public int SortOrder { get; set; }

    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
