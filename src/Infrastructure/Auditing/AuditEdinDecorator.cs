using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.Edin;

namespace SamorodinkaTech.Fiducia.Infrastructure.Auditing;

/// <summary>
/// Декоратор для IEdinApiClient — логирует обращения к ЕДИН через ISecurityAuditService.
/// </summary>
public class AuditEdinDecorator : IEdinApiClient
{
    private readonly IEdinApiClient _inner;
    private readonly ISecurityAuditService _auditService;
    private readonly IClientIpProvider _ipProvider;
    private readonly ILogger<AuditEdinDecorator> _logger;

    public AuditEdinDecorator(
        IEdinApiClient inner,
        ISecurityAuditService auditService,
        IClientIpProvider ipProvider,
        ILogger<AuditEdinDecorator> logger)
    {
        _inner = inner;
        _auditService = auditService;
        _ipProvider = ipProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<EdinPersonResult?> ResolvePersonAsync(
        string lastName, string firstName, string? middleName,
        string? inn, string? snils,
        string? dulType, string? dulSeries, string? dulNumber,
        CancellationToken cancellationToken = default)
    {
        var clientIp = _ipProvider.GetClientIp();
        try
        {
            var result = await _inner.ResolvePersonAsync(
                lastName, firstName, middleName,
                inn, snils, dulType, dulSeries, dulNumber,
                cancellationToken);

            await _auditService.LogEventAsync("EXTERNAL:Edin:Resolve", clientIp,
                $"Идентификация ЕДИН: {lastName} {firstName}, ИНН={inn ?? "-"}, СНИЛС={snils ?? "-"}, " +
                $"статус={result?.Status ?? "null"}, MasterId={result?.MasterId?.ToString() ?? "-"}",
                entityName: "Edin");

            return result;
        }
        catch (Exception ex)
        {
            await _auditService.LogEventAsync("EXTERNAL:Edin:Resolve", clientIp,
                $"Идентификация ЕДИН: {lastName} {firstName}, ошибка={ex.Message}",
                entityName: "Edin");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<EdinPersonResult?> GetPersonAsync(
        Guid masterId,
        CancellationToken cancellationToken = default)
    {
        var clientIp = _ipProvider.GetClientIp();
        try
        {
            var result = await _inner.GetPersonAsync(masterId, cancellationToken);

            await _auditService.LogEventAsync("EXTERNAL:Edin:GetPerson", clientIp,
                $"Запрос данных ЕДИН: MasterId={masterId}, результат={result?.Status ?? "null"}",
                entityName: "Edin");

            return result;
        }
        catch (Exception ex)
        {
            await _auditService.LogEventAsync("EXTERNAL:Edin:GetPerson", clientIp,
                $"Запрос данных ЕДИН: MasterId={masterId}, ошибка={ex.Message}",
                entityName: "Edin");
            throw;
        }
    }
}
