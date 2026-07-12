namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Реальный этап организационного мероприятия (org_stages), созданный из шаблона tpl_org_stages.
/// </summary>
public class OrgStage
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор цели (intent_id).</summary>
    public Guid IntentId { get; set; }

    /// <summary>Цель.</summary>
    public OrgIntent? Intent { get; set; }

    /// <summary>Идентификатор шаблона (template_stage_id).</summary>
    public Guid? TemplateStageId { get; set; }

    /// <summary>Шаблон.</summary>
    public TplOrgStage? TemplateStage { get; set; }

    /// <summary>Наименование этапа (name).</summary>
    public string Name { get; set; } = default!;

    /// <summary>Описание (description).</summary>
    public string? Description { get; set; }

    /// <summary>Порядок сортировки (sort_order).</summary>
    public int SortOrder { get; set; }

    /// <summary>Статус (status).</summary>
    public string Status { get; set; } = "PLANNED";

    /// <summary>Фактическая дата начала (actual_start).</summary>
    public DateOnly? ActualStart { get; set; }

    /// <summary>Фактическая дата завершения (actual_end).</summary>
    public DateOnly? ActualEnd { get; set; }

    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Оферы, относящиеся к данному этапу.</summary>
    public ICollection<OrgOffer>? Offers { get; set; }
}