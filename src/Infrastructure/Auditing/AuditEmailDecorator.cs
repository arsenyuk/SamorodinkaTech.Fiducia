using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.Email;

namespace SamorodinkaTech.Fiducia.Infrastructure.Auditing;

/// <summary>
/// Декоратор для IEmailService — логирует отправку писем в аудит.
/// </summary>
public class AuditEmailDecorator : IEmailService
{
    private readonly IEmailService _inner;
    private readonly ISecurityAuditService _auditService;
    private readonly IClientIpProvider _ipProvider;
    private readonly ILogger<AuditEmailDecorator> _logger;

    /// <summary>
    /// Создаёт экземпляр декоратора аудита email.
    /// </summary>
    public AuditEmailDecorator(
        IEmailService inner,
        ISecurityAuditService auditService,
        IClientIpProvider ipProvider,
        ILogger<AuditEmailDecorator> logger)
    {
        _inner = inner;
        _auditService = auditService;
        _ipProvider = ipProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var clientIp = _ipProvider.GetClientIp();
        try
        {
            await _inner.SendAsync(message, cancellationToken);
            await _auditService.LogEventAsync("EXTERNAL:Email:Send", clientIp,
                $"Отправка email: To={message.To}, Subject={message.Subject}",
                entityName: "Email");
        }
        catch (Exception ex)
        {
            await _auditService.LogEventAsync("EXTERNAL:Email:Send", clientIp,
                $"Ошибка отправки email: To={message.To}, Subject={message.Subject}, ошибка={ex.Message}",
                entityName: "Email");
            throw;
        }
    }
}
