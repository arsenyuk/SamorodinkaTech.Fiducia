using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.Spark;

namespace SamorodinkaTech.Fiducia.Infrastructure.Auditing;

/// <summary>
/// Декоратор для ISparkApiClient — логирует обращение к СПАРК API.
/// Авторизация происходит внутри клиента (Authmethod), поэтому логируется первый запрос.
/// </summary>
public class AuditSparkDecorator : ISparkApiClient
{
    private readonly ISparkApiClient _inner;
    private readonly ISecurityAuditService _auditService;
    private readonly ILogger<AuditSparkDecorator> _logger;

    public AuditSparkDecorator(
        ISparkApiClient inner,
        ISecurityAuditService auditService,
        ILogger<AuditSparkDecorator> logger)
    {
        _inner = inner;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<SparkCompany?> GetCompanyByInnAsync(
        string inn,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _inner.GetCompanyByInnAsync(inn, cancellationToken);
            await _auditService.LogEventAsync("EXTERNAL_AUTH:Spark", "internal",
                $"Обращение к СПАРК API: GetCompanyByInn inn={inn}, результат={(
                    result != null ? "найдено" : "не найдено")}",
                entityName: "Spark");
            return result;
        }
        catch (Exception ex)
        {
            await _auditService.LogEventAsync("EXTERNAL_AUTH:Spark", "internal",
                $"Обращение к СПАРК API: GetCompanyByInn inn={inn}, ошибка={ex.Message}",
                entityName: "Spark");
            throw;
        }
    }

    public async Task<SparkManager?> GetManagerAsync(
        string inn,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _inner.GetManagerAsync(inn, cancellationToken);
            await _auditService.LogEventAsync("EXTERNAL_AUTH:Spark", "internal",
                $"Обращение к СПАРК API: GetManager inn={inn}, результат={(
                    result != null ? "найдено" : "не найдено")}",
                entityName: "Spark");
            return result;
        }
        catch (Exception ex)
        {
            await _auditService.LogEventAsync("EXTERNAL_AUTH:Spark", "internal",
                $"Обращение к СПАРК API: GetManager inn={inn}, ошибка={ex.Message}",
                entityName: "Spark");
            throw;
        }
    }

    public async Task<List<SparkFounder>> GetFoundersAsync(
        string inn,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _inner.GetFoundersAsync(inn, cancellationToken);
            await _auditService.LogEventAsync("EXTERNAL_AUTH:Spark", "internal",
                $"Обращение к СПАРК API: GetFounders inn={inn}, результат={result.Count} записей",
                entityName: "Spark");
            return result;
        }
        catch (Exception ex)
        {
            await _auditService.LogEventAsync("EXTERNAL_AUTH:Spark", "internal",
                $"Обращение к СПАРК API: GetFounders inn={inn}, ошибка={ex.Message}",
                entityName: "Spark");
            throw;
        }
    }
}
