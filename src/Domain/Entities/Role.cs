namespace SamorodinkaTech.Fiducia.Domain.Entities;

public class Role
{
    /// <summary>Уникальный идентификатор (id).</summary>
    public Guid Id { get; set; }
    /// <summary>Код роли (code).</summary>
    public string Code { get; set; } = string.Empty;
    /// <summary>Наименование роли (name).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Связи пользователей с этой ролью.</summary>
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
