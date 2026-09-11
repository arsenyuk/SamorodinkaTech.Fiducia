using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.PochtaRussia;

namespace SamorodinkaTech.Fiducia.Infrastructure.Auditing;

/// <summary>
/// Декоратор для IPochtaRussiaApiClient — логирует обращения к API «Отправка» Почты России.
/// </summary>
public class AuditPochtaRussiaDecorator : IPochtaRussiaApiClient
{
    private readonly IPochtaRussiaApiClient _inner;
    private readonly ISecurityAuditService _auditService;
    private readonly IClientIpProvider _ipProvider;
    private readonly ILogger<AuditPochtaRussiaDecorator> _logger;

    public AuditPochtaRussiaDecorator(
        IPochtaRussiaApiClient inner,
        ISecurityAuditService auditService,
        IClientIpProvider ipProvider,
        ILogger<AuditPochtaRussiaDecorator> logger)
    {
        _inner = inner;
        _auditService = auditService;
        _ipProvider = ipProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PochtaRussiaRateResponse?> CalculateRateAsync(
        PochtaRussiaRateRequest request,
        CancellationToken cancellationToken = default)
    {
        var clientIp = _ipProvider.GetClientIp();
        try
        {
            var result = await _inner.CalculateRateAsync(request, cancellationToken);
            await _auditService.LogEventAsync("EXTERNAL:PochtaRussia:RateCalc", clientIp,
                $"Расчёт тарифа Почты России: MailType={request.MailType}, Mass={request.Mass}, результат={result?.TotalRate} коп.",
                entityName: "PochtaRussia");
            return result;
        }
        catch (Exception ex)
        {
            await _auditService.LogEventAsync("EXTERNAL:PochtaRussia:RateCalc", clientIp,
                $"Расчёт тарифа Почты России: MailType={request.MailType}, ошибка={ex.Message}",
                entityName: "PochtaRussia");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<PochtaRussiaAddressResponse>> NormalizeAddressesAsync(
        IReadOnlyList<PochtaRussiaAddressRequest> addresses,
        CancellationToken cancellationToken = default)
    {
        var clientIp = _ipProvider.GetClientIp();
        try
        {
            var result = await _inner.NormalizeAddressesAsync(addresses, cancellationToken);
            await _auditService.LogEventAsync("EXTERNAL:PochtaRussia:NormalizeAddress", clientIp,
                $"Нормализация адресов Почты России: {addresses.Count} адресов, результат={result.Count} записей",
                entityName: "PochtaRussia");
            return result;
        }
        catch (Exception ex)
        {
            await _auditService.LogEventAsync("EXTERNAL:PochtaRussia:NormalizeAddress", clientIp,
                $"Нормализация адресов Почты России: {addresses.Count} адресов, ошибка={ex.Message}",
                entityName: "PochtaRussia");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<PochtaRussiaOrderResponse?> CreateOrderAsync(
        PochtaRussiaOrderRequest order,
        CancellationToken cancellationToken = default)
    {
        var clientIp = _ipProvider.GetClientIp();
        try
        {
            var result = await _inner.CreateOrderAsync(order, cancellationToken);
            await _auditService.LogEventAsync("EXTERNAL:PochtaRussia:CreateOrder", clientIp,
                $"Создание заказа Почты России: OrderNum={order.OrderNum}, MailType={order.MailType}, результат={result?.ResultIds.Count} отправлений",
                entityName: "PochtaRussia");
            return result;
        }
        catch (Exception ex)
        {
            await _auditService.LogEventAsync("EXTERNAL:PochtaRussia:CreateOrder", clientIp,
                $"Создание заказа Почты России: OrderNum={order.OrderNum}, ошибка={ex.Message}",
                entityName: "PochtaRussia");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<PochtaRussiaApiLimitResponse?> GetApiLimitAsync(
        CancellationToken cancellationToken = default)
    {
        var clientIp = _ipProvider.GetClientIp();
        try
        {
            var result = await _inner.GetApiLimitAsync(cancellationToken);
            await _auditService.LogEventAsync("EXTERNAL:PochtaRussia:ApiLimit", clientIp,
                $"Лимиты API Почты России: allowed={result?.AllowedCount}, current={result?.CurrentCount}",
                entityName: "PochtaRussia");
            return result;
        }
        catch (Exception ex)
        {
            await _auditService.LogEventAsync("EXTERNAL:PochtaRussia:ApiLimit", clientIp,
                $"Лимиты API Почты России: ошибка={ex.Message}",
                entityName: "PochtaRussia");
            throw;
        }
    }
}
