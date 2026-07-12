namespace SamorodinkaTech.Fiducia.Domain.Entities;

public class Role
{
    public Guid Id { get; set; }
    public string RoleCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
