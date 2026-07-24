namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Задача организационного мероприятия (tpl_org_tasks).
/// Четвёртый уровень иерархии, привязан к OrgOffer.
/// </summary>
public class TplOrgTask
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор офера (offer_id).</summary>
    public Guid OfferId { get; set; }

    /// <summary>Офер.</summary>
    public TplOrgOffer? Offer { get; set; }

    /// <summary>Наименование задачи (name).</summary>
    public string Name { get; set; } = default!;

    /// <summary>Описание (description).</summary>
    public string? Description { get; set; }

    /// <summary>Порядок сортировки (sort_order).</summary>
    public int SortOrder { get; set; }

    /// <summary>Идентификатор роли исполнителя (assigned_role_id).</summary>
    public Guid? AssignedRoleId { get; set; }

    /// <summary>Роль исполнителя.</summary>
    public Role? AssignedRole { get; set; }

    /// <summary>Идентификатор должности в Совете директоров (assigned_board_role_id).</summary>
    public Guid? AssignedBoardRoleId { get; set; }

    /// <summary>Должность в Совете директоров.</summary>
    public BoardRole? AssignedBoardRole { get; set; }

    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
