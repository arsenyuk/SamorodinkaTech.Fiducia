namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Шаблон: связь офера с ролью — пул кандидатов (tpl_org_offer_roles).
/// </summary>
public class TplOrgOfferRole
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор шаблона офера (tpl_offer_id).</summary>
    public Guid TplOfferId { get; set; }

    /// <summary>Шаблон офера.</summary>
    public TplOrgOffer? Offer { get; set; }

    /// <summary>Идентификатор роли (role_id).</summary>
    public Guid RoleId { get; set; }

    /// <summary>Роль.</summary>
    public Role? Role { get; set; }

    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
