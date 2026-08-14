namespace SamorodinkaTech.Fiducia.Domain.Entities;

public class RefRole
{
    /// <summary>Уникальный идентификатор (id).</summary>
    public Guid Id { get; set; }
    /// <summary>Код роли (code).</summary>
    public string Code { get; set; } = string.Empty;
    /// <summary>Наименование роли (name).</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>Идентификатор создателя (created_by).</summary>
    public Guid CreatedBy { get; set; }

    /// <summary>Связи пользователей с этой ролью.</summary>
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
