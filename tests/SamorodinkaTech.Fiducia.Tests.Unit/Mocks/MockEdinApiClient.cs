using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.Edin;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Mocks;

/// <summary>
/// Mock-реализация IEdinApiClient для unit-тестирования.
/// Возвращает настраиваемый результат resolve.
/// </summary>
public class MockEdinApiClient : IEdinApiClient
{
    /// <summary>Результат, возвращаемый ResolvePersonAsync.</summary>
    public EdinPersonResult? ResolveResult { get; set; }

    /// <summary>Результат, возвращаемый GetPersonAsync.</summary>
    public EdinPersonResult? GetPersonResult { get; set; }

    /// <summary>Количество вызовов ResolvePersonAsync.</summary>
    public int ResolveCallCount { get; private set; }

    /// <summary>Последний вызванный lastName.</summary>
    public string? LastResolveLastName { get; private set; }

    /// <summary>Последний вызванный inn.</summary>
    public string? LastResolveInn { get; private set; }

    /// <summary>Если true, все методы возвращают null (имитация недоступности сервиса).</summary>
    public bool SimulateUnavailable { get; set; }

    public Task<EdinPersonResult?> ResolvePersonAsync(
        string lastName, string firstName, string? middleName,
        string? inn, string? snils,
        string? dulType, string? dulSeries, string? dulNumber,
        CancellationToken cancellationToken = default)
    {
        ResolveCallCount++;
        LastResolveLastName = lastName;
        LastResolveInn = inn;

        if (SimulateUnavailable)
            return Task.FromResult<EdinPersonResult?>(null);

        return Task.FromResult(ResolveResult);
    }

    public Task<EdinPersonResult?> GetPersonAsync(
        Guid masterId,
        CancellationToken cancellationToken = default)
    {
        if (SimulateUnavailable)
            return Task.FromResult<EdinPersonResult?>(null);

        return Task.FromResult(GetPersonResult);
    }
}
