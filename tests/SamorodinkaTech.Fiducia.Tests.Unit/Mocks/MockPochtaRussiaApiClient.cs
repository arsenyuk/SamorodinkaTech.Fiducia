using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.PochtaRussia;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Mocks;

/// <summary>
/// Mock-реализация IPochtaRussiaApiClient для unit-тестирования.
/// </summary>
public class MockPochtaRussiaApiClient : IPochtaRussiaApiClient
{
    /// <summary>Результат, возвращаемый CalculateRateAsync.</summary>
    public PochtaRussiaRateResponse? RateResult { get; set; }

    /// <summary>Результат, возвращаемый NormalizeAddressesAsync.</summary>
    public List<PochtaRussiaAddressResponse> NormalizeResult { get; set; } = new();

    /// <summary>Результат, возвращаемый CreateOrderAsync.</summary>
    public PochtaRussiaOrderResponse? OrderResult { get; set; }

    /// <summary>Результат, возвращаемый GetApiLimitAsync.</summary>
    public PochtaRussiaApiLimitResponse? ApiLimitResult { get; set; }

    /// <summary>Количество вызовов CalculateRateAsync.</summary>
    public int RateCallCount { get; private set; }

    /// <summary>Количество вызовов NormalizeAddressesAsync.</summary>
    public int NormalizeCallCount { get; private set; }

    /// <summary>Количество вызовов CreateOrderAsync.</summary>
    public int OrderCallCount { get; private set; }

    /// <summary>Количество вызовов GetApiLimitAsync.</summary>
    public int ApiLimitCallCount { get; private set; }

    /// <summary>Последний вызванный MailType в CalculateRateAsync.</summary>
    public string? LastRateMailType { get; private set; }

    /// <summary>Последний вызванный OrderNum в CreateOrderAsync.</summary>
    public string? LastOrderNum { get; private set; }

    /// <summary>Если true, все методы возвращают null (имитация недоступности сервиса).</summary>
    public bool SimulateUnavailable { get; set; }

    /// <summary>Если true, все методы выбрасывают исключение (имитация сбоя API).</summary>
    public bool SimulateFailure { get; set; }

    public Task<PochtaRussiaRateResponse?> CalculateRateAsync(
        PochtaRussiaRateRequest request,
        CancellationToken cancellationToken = default)
    {
        RateCallCount++;
        LastRateMailType = request.MailType;

        if (SimulateFailure)
            throw new HttpRequestException("Simulated Pochta Russia API failure");

        if (SimulateUnavailable)
            return Task.FromResult<PochtaRussiaRateResponse?>(null);

        return Task.FromResult(RateResult);
    }

    public Task<List<PochtaRussiaAddressResponse>> NormalizeAddressesAsync(
        IReadOnlyList<PochtaRussiaAddressRequest> addresses,
        CancellationToken cancellationToken = default)
    {
        NormalizeCallCount++;

        if (SimulateFailure)
            throw new HttpRequestException("Simulated Pochta Russia API failure");

        if (SimulateUnavailable)
            return Task.FromResult(new List<PochtaRussiaAddressResponse>());

        return Task.FromResult(NormalizeResult);
    }

    public Task<PochtaRussiaOrderResponse?> CreateOrderAsync(
        PochtaRussiaOrderRequest order,
        CancellationToken cancellationToken = default)
    {
        OrderCallCount++;
        LastOrderNum = order.OrderNum;

        if (SimulateFailure)
            throw new HttpRequestException("Simulated Pochta Russia API failure");

        if (SimulateUnavailable)
            return Task.FromResult<PochtaRussiaOrderResponse?>(null);

        return Task.FromResult(OrderResult);
    }

    public Task<PochtaRussiaApiLimitResponse?> GetApiLimitAsync(
        CancellationToken cancellationToken = default)
    {
        ApiLimitCallCount++;

        if (SimulateFailure)
            throw new HttpRequestException("Simulated Pochta Russia API failure");

        if (SimulateUnavailable)
            return Task.FromResult<PochtaRussiaApiLimitResponse?>(null);

        return Task.FromResult(ApiLimitResult);
    }
}
