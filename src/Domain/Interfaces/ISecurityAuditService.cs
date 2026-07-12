namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Сервис регистрации событий безопасности (РСБ.2).
/// </summary>
public interface ISecurityAuditService
{
    /// <summary>
    /// Регистрирует событие безопасности.
    /// </summary>
    Task LogEventAsync(
        string actionCode,
        string userIp,
        string description,
        Guid? userId = null,
        string? entityName = null,
        Guid? entityId = null,
        CancellationToken cancellationToken = default);
}
