using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Interfaces;

namespace SamorodinkaTech.Fiducia.Infrastructure.Auditing;

/// <summary>
/// Реализация сервиса регистрации событий безопасности.
/// Записывает события в файловый лог (через Serilog sub-logger).
/// Формат записи: [AUDIT] {ActionCode} | User={UserId} IP={UserIp} | {Description} | {EntityName} {EntityId}
/// </summary>
public class SecurityAuditService : ISecurityAuditService
{
    private readonly ILogger<SecurityAuditService> _logger;

    public SecurityAuditService(ILogger<SecurityAuditService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task LogEventAsync(
        string actionCode,
        string userIp,
        string description,
        Guid? userId = null,
        string? entityName = null,
        Guid? entityId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userPart = userId.HasValue ? userId.Value.ToString("N")[..8] : "anonymous";
            var entityShort = entityId.HasValue ? entityId.Value.ToString("N")[..8] : "";
            var entityPart = entityId.HasValue ? $"{entityName} {entityShort}" : entityName ?? "";

            using (_logger.BeginScope(new Dictionary<string, object> { ["AuditLog"] = true }))
            {
                _logger.LogInformation(
                    "[AUDIT] {ActionCode} | User={UserPart} IP={UserIp} | {Description} | {EntityPart}",
                    actionCode, userPart, userIp, description, entityPart);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка записи аудита в файл: {ActionCode}", actionCode);
        }

        await Task.CompletedTask;
    }
}
