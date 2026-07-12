using System.Text.Json;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Interfaces;

namespace SamorodinkaTech.Fiducia.Infrastructure.Auditing;

/// <summary>
/// Реализация сервиса регистрации событий безопасности.
/// Записывает события в файловый лог (через Serilog sub-logger).
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
        var timestamp = DateTime.UtcNow;

        try
        {
            var entry = JsonSerializer.Serialize(new
            {
                timestamp,
                actionCode,
                userIp,
                userId,
                entityName,
                entityId,
                description
            });
            using (_logger.BeginScope(new Dictionary<string, object> { ["AuditLog"] = true }))
            {
                _logger.LogInformation("{AuditEntry}", entry);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка записи аудита в файл: {ActionCode}", actionCode);
        }

        await Task.CompletedTask;
    }
}
