namespace SamorodinkaTech.Fiducia.Domain.Entities;

public class UserRole
{
    /// <summary>Уникальный идентификатор (id).</summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>Идентификатор пользователя (user_id).</summary>
    public Guid UserId { get; set; }
    /// <summary>Пользователь.</summary>
    public User User { get; set; } = null!;

    /// <summary>Идентификатор роли (role_id).</summary>
    public Guid RoleId { get; set; }
    /// <summary>Роль.</summary>
    public Role Role { get; set; } = null!;
}
