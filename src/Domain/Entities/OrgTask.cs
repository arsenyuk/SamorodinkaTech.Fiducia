using SamorodinkaTech.Fiducia.Domain.Enums;

namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Реальная задача организационного мероприятия (org_tasks), созданная из шаблона tpl_org_offers.
/// </summary>
public class OrgTask
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
    public TplOrgTaskOffer? TemplateOffer { get; set; }

    /// <summary>JSON-массив role_id кандидатов (candidate_roles).</summary>
    public string? CandidateRoles { get; set; }

    /// <summary>JSON-массив ID задач-предшественников (predecessor_task_ids).</summary>
    public string? PredecessorTaskIds { get; set; }

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
    public RefRole? AssignedRole { get; set; }

    /// <summary>Идентификатор должности в Совете директоров (assigned_board_role_id).</summary>
    public Guid? AssignedBoardRoleId { get; set; }

    /// <summary>Должность в Совете директоров.</summary>
    public RefBoardRole? AssignedBoardRole { get; set; }

    /// <summary>Фактическая дата начала (actual_start).</summary>
    public DateOnly? ActualStart { get; set; }

    /// <summary>Фактическая дата завершения (actual_end).</summary>
    public DateOnly? ActualEnd { get; set; }

    /// <summary>Тип зависимости: FS — Финиш-Старт, SS — Старт-Старт (dependency_type).</summary>
    public DependencyType DependencyType { get; set; } = DependencyType.FS;

    /// <summary>Плановая дата начала (planned_start).</summary>
    public DateOnly? PlannedStart { get; set; }

    /// <summary>Плановая дата завершения (planned_end).</summary>
    public DateOnly? PlannedEnd { get; set; }

    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}