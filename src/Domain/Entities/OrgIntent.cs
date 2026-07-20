namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Реальная цель организационного мероприятия (org_intents), созданная из шаблона tpl_org_intents.
/// Привязана к конкретному юридическому лицу.
/// </summary>
public class OrgIntent
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор ЮЛ (legal_entity_id).</summary>
    public Guid LegalEntityId { get; set; }

    /// <summary>Юридическое лицо.</summary>
    public LegalEntity? LegalEntity { get; set; }

    /// <summary>Идентификатор шаблона (template_intent_id).</summary>
    public Guid? TemplateIntentId { get; set; }

    /// <summary>Шаблон.</summary>
    public TplOrgIntent? TemplateIntent { get; set; }

    /// <summary>Наименование цели (name).</summary>
    public string Name { get; set; } = default!;

    /// <summary>Описание (description).</summary>
    public string? Description { get; set; }

    /// <summary>Порядок сортировки (sort_order).</summary>
    public int SortOrder { get; set; }

    /// <summary>Статус: PLANNED, IN_PROGRESS, COMPLETED, CANCELLED (status).</summary>
    public string Status { get; set; } = "PLANNED";

    /// <summary>Фактическая дата начала (actual_start).</summary>
    public DateOnly? ActualStart { get; set; }

    /// <summary>Фактическая дата завершения (actual_end).</summary>
    public DateOnly? ActualEnd { get; set; }

    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Этапы, относящиеся к данной цели.</summary>
    public ICollection<OrgStage>? Stages { get; set; }
}