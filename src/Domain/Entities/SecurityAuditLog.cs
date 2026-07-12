namespace SamorodinkaTech.Fiducia.Domain.Entities;

public class SecurityAuditLog
{
    public long Id { get; set; }
    public Guid? UserId { get; set; }
    public string UserIp { get; set; } = string.Empty;
    public string ActionCode { get; set; } = string.Empty;
    public string? EntityName { get; set; }
    public Guid? EntityId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime LogTimestamp { get; set; } = DateTime.UtcNow;
}
