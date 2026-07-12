namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Реальный офер организационного мероприятия (org_offers), созданный из шаблона tpl_org_offers.
/// </summary>
public class OrgOffer
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор этапа (stage_id).</summary>
    public Guid StageId { get; set; }

    /// <summary>Этап.</summary>
    public OrgStage? Stage { get; set; }

    /// <summary>Идентификатор шаблона (template_offer_id).</summary>
    public Guid? TemplateOfferId { get; set; }

    /// <summary>Шаблон.</summary>
    public TplOrgOffer? TemplateOffer { get; set; }

    /// <summary>Наименование офера (name).</summary>
    public string Name { get; set; } = default!;

    /// <summary>Описание (description).</summary>
    public string? Description { get; set; }

    /// <summary>Порядок сортировки (sort_order).</summary>
    public int SortOrder { get; set; }

    /// <summary>Статус (status).</summary>
    public string Status { get; set; } = "PLANNED";

    /// <summary>Идентификатор назначенного пользователя (assigned_user_id).</summary>
    public Guid? AssignedUserId { get; set; }

    /// <summary>Назначенный пользователь.</summary>
    public User? AssignedUser { get; set; }

    /// <summary>JSON-массив role_id кандидатов (candidate_roles).</summary>
    public string? CandidateRoles { get; set; }

    /// <summary>Фактическая дата начала (actual_start).</summary>
    public DateOnly? ActualStart { get; set; }

    /// <summary>Фактическая дата завершения (actual_end).</summary>
    public DateOnly? ActualEnd { get; set; }

    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Задачи, относящиеся к данному оферу.</summary>
    public ICollection<OrgTask>? Tasks { get; set; }
}