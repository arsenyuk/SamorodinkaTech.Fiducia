namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Реальная задача организационного мероприятия (org_tasks), созданная из шаблона tpl_org_tasks.
/// </summary>
public class OrgTask
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор офера (offer_id).</summary>
    public Guid OfferId { get; set; }

    /// <summary>Офер.</summary>
    public OrgOffer? Offer { get; set; }

    /// <summary>Идентификатор шаблона (template_task_id).</summary>
    public Guid? TemplateTaskId { get; set; }

    /// <summary>Шаблон.</summary>
    public TplOrgTask? TemplateTask { get; set; }

    /// <summary>Наименование задачи (name).</summary>
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

    /// <summary>Идентификатор роли исполнителя (assigned_role_id).</summary>
    public Guid? AssignedRoleId { get; set; }

    /// <summary>Роль исполнителя.</summary>
    public Role? AssignedRole { get; set; }

    /// <summary>Идентификатор должности в Совете директоров (assigned_board_role_id).</summary>
    public Guid? AssignedBoardRoleId { get; set; }

    /// <summary>Должность в Совете директоров.</summary>
    public BoardRole? AssignedBoardRole { get; set; }

    /// <summary>Фактическая дата начала (actual_start).</summary>
    public DateOnly? ActualStart { get; set; }

    /// <summary>Фактическая дата завершения (actual_end).</summary>
    public DateOnly? ActualEnd { get; set; }

    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}