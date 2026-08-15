using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.CbrFinOrg;

namespace SamorodinkaTech.Fiducia.Infrastructure.Auditing;

/// <summary>
/// Декоратор для ICbrFinOrgClient — логирует обращение к SOAP-сервису ЦБ РФ (FinOrg).
/// </summary>
public class AuditCbrFinOrgDecorator : ICbrFinOrgClient
{
    private readonly ICbrFinOrgClient _inner;
    private readonly ISecurityAuditService _auditService;
    private readonly ILogger<AuditCbrFinOrgDecorator> _logger;

    public AuditCbrFinOrgDecorator(
        ICbrFinOrgClient inner,
        ISecurityAuditService auditService,
        ILogger<AuditCbrFinOrgDecorator> logger)
    {
        _inner = inner;
        _auditService = auditService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<CbrFinOrgOrganization?> GetOrganizationByInnAsync(
        long inn,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _inner.GetOrganizationByInnAsync(inn, cancellationToken);
            await _auditService.LogEventAsync("EXTERNAL_AUTH:CbrFinOrg", "internal",
                $"Обращение к FinOrg API: GetFullInfoByINN inn={inn}, результат={(
                    result != null ? "найдено" : "не найдено")}",
                entityName: "CbrFinOrg");
            return result;
        }
        catch (Exception ex)
        {
            await _auditService.LogEventAsync("EXTERNAL_AUTH:CbrFinOrg", "internal",
                $"Обращение к FinOrg API: GetFullInfoByINN inn={inn}, ошибка={ex.Message}",
                entityName: "CbrFinOrg");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<CbrFinOrgOrganization?> GetOrganizationByOgrnAsync(
        long ogrn,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _inner.GetOrganizationByOgrnAsync(ogrn, cancellationToken);
            await _auditService.LogEventAsync("EXTERNAL_AUTH:CbrFinOrg", "internal",
                $"Обращение к FinOrg API: GetFullInfoByOGRN ogrn={ogrn}, результат={(
                    result != null ? "найдено" : "не найдено")}",
                entityName: "CbrFinOrg");
            return result;
        }
        catch (Exception ex)
        {
            await _auditService.LogEventAsync("EXTERNAL_AUTH:CbrFinOrg", "internal",
                $"Обращение к FinOrg API: GetFullInfoByOGRN ogrn={ogrn}, ошибка={ex.Message}",
                entityName: "CbrFinOrg");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<CbrFinOrgSearchResult> SearchAsync(
        string? name,
        string? address,
        int page = 0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _inner.SearchAsync(name, address, page, cancellationToken);
            await _auditService.LogEventAsync("EXTERNAL_AUTH:CbrFinOrg", "internal",
                $"Обращение к FinOrg API: Search name={name ?? "*"}, addr={address ?? "*"}, " +
                $"страница={page}, результат={result.TotalRows} записей, успех={result.IsSuccess}",
                entityName: "CbrFinOrg");
            return result;
        }
        catch (Exception ex)
        {
            await _auditService.LogEventAsync("EXTERNAL_AUTH:CbrFinOrg", "internal",
                $"Обращение к FinOrg API: Search name={name ?? "*"}, ошибка={ex.Message}",
                entityName: "CbrFinOrg");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<CbrFinOrgRecord>> SearchByInnsAsync(
        long[] inns,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _inner.SearchByInnsAsync(inns, cancellationToken);
            await _auditService.LogEventAsync("EXTERNAL_AUTH:CbrFinOrg", "internal",
                $"Обращение к FinOrg API: SearchByINNs count={inns.Length}, " +
                $"результат={result.Count} записей",
                entityName: "CbrFinOrg");
            return result;
        }
        catch (Exception ex)
        {
            await _auditService.LogEventAsync("EXTERNAL_AUTH:CbrFinOrg", "internal",
                $"Обращение к FinOrg API: SearchByINNs count={inns.Length}, ошибка={ex.Message}",
                entityName: "CbrFinOrg");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<DateTime> GetLastUpdateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _inner.GetLastUpdateAsync(cancellationToken);
            await _auditService.LogEventAsync("EXTERNAL_AUTH:CbrFinOrg", "internal",
                $"Обращение к FinOrg API: GetLastUpdate, результат={result:O}",
                entityName: "CbrFinOrg");
            return result;
        }
        catch (Exception ex)
        {
            await _auditService.LogEventAsync("EXTERNAL_AUTH:CbrFinOrg", "internal",
                $"Обращение к FinOrg API: GetLastUpdate, ошибка={ex.Message}",
                entityName: "CbrFinOrg");
            throw;
        }
    }
}
