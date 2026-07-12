using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;

namespace SamorodinkaTech.Fiducia.Infrastructure.Auditing;

/// <summary>
/// Реализация сервиса регистрации событий безопасности (РСБ.2 + РСБ.3).
/// Записывает события в PostgreSQL и файловый лог (через Serilog sub-logger).
/// </summary>
public class SecurityAuditService : ISecurityAuditService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<SecurityAuditService> _logger;
    private readonly bool _writeToDb;
    private readonly bool _writeToFile;

    public SecurityAuditService(
        IApplicationDbContext context,
        ILogger<SecurityAuditService> logger,
        IConfiguration configuration)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _writeToDb = configuration.GetValue<bool>("SecurityAudit:WriteToDatabase", true);
        _writeToFile = configuration.GetValue<bool>("SecurityAudit:WriteToFile", true);
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

        // Запись в БД
        if (_writeToDb)
        {
            try
            {
                var entry = new SecurityAuditLog
                {
                    UserId = userId,
                    UserIp = userIp,
                    ActionCode = actionCode,
                    EntityName = entityName,
                    EntityId = entityId,
                    Description = description,
                    LogTimestamp = timestamp
                };
                _context.SecurityAuditLogs.Add(entry);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка записи аудита в БД: {ActionCode}", actionCode);
            }
        }

        // Запись в файл — через Serilog с пометкой AuditLog, чтобы
        // main file sink мог исключить аудит из app-лога по фильтру
        if (_writeToFile)
        {
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
        }
    }
}
