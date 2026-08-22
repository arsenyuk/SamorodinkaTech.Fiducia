using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SamorodinkaTech.Fiducia.Domain.Interfaces;

namespace SamorodinkaTech.Fiducia.Infrastructure.Auditing;

/// <summary>
/// Реализация сервиса регистрации событий безопасности.
/// Записывает события напрямую в файл аудита.
/// Формат записи: [AUDIT] {ActionCode} | User={UserId} IP={UserIp} | {Description} | {EntityName} {EntityId}
/// </summary>
public class SecurityAuditService : ISecurityAuditService
{
    private readonly ILogger<SecurityAuditService> _logger;
    private readonly string _auditFilePath;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public SecurityAuditService(
        ILogger<SecurityAuditService> logger,
        IOptions<SecurityAuditOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        var opts = options?.Value ?? throw new ArgumentNullException(nameof(options));

        var storagePath = opts.StoragePath ?? Path.Combine(AppContext.BaseDirectory, "logs", "audit");
        Directory.CreateDirectory(storagePath);

        var fileName = $"audit-{DateTime.UtcNow:yyyyMMddHH}.log";
        _auditFilePath = Path.Combine(storagePath, fileName);
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

            var message = $"[AUDIT] {actionCode} | User={userPart} IP={userIp} | {description} | {entityPart}";

            await _lock.WaitAsync(cancellationToken);
            try
            {
                await File.AppendAllTextAsync(_auditFilePath, message + Environment.NewLine, Encoding.UTF8, cancellationToken);
            }
            finally
            {
                _lock.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ошибка записи аудита в файл: {ActionCode}", actionCode);
        }
    }
}

/// <summary>
/// Опции конфигурации аудита безопасности.
/// </summary>
public class SecurityAuditOptions
{
    /// <summary>Путь к директории для файлов аудита.</summary>
    public string? StoragePath { get; set; }
}
