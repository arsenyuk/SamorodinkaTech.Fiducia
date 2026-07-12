using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.Spark;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Mocks;

/// <summary>
/// Mock-реализация ISparkApiClient для unit-тестирования.
/// Хранит данные в оперативной памяти и имитирует поведение СПАРК API.
/// </summary>
public class MockSparkApiClient : ISparkApiClient
{
    private readonly Dictionary<string, SparkCompany> _companies = new();
    private readonly Dictionary<string, SparkManager> _managers = new();

    /// <summary>Задержка ответа в миллисекундах для имитации сети (по умолчанию 0).</summary>
    public int SimulatedDelayMs { get; set; }

    /// <summary>Если true, все методы выбрасывают исключение (имитация сбоя API).</summary>
    public bool SimulateFailure { get; set; }

    /// <inheritdoc />
    public async Task<SparkCompany?> GetCompanyByInnAsync(
        string inn,
        CancellationToken cancellationToken = default)
    {
        await MaybeDelay();
        ThrowIfFailure();

        _companies.TryGetValue(inn, out var company);
        return company;
    }

    /// <inheritdoc />
    public async Task<SparkManager?> GetManagerAsync(
        string inn,
        CancellationToken cancellationToken = default)
    {
        await MaybeDelay();
        ThrowIfFailure();

        _managers.TryGetValue(inn, out var manager);
        return manager;
    }

    /// <summary>
    /// Добавляет компанию в mock-хранилище (для настройки тестовых данных).
    /// </summary>
    public void AddCompany(SparkCompany company)
    {
        _companies[company.Inn] = company;
    }

    /// <summary>
    /// Добавляет руководителя в mock-хранилище (для настройки тестовых данных).
    /// </summary>
    public void AddManager(string inn, SparkManager manager)
    {
        _managers[inn] = manager;
    }

    private async Task MaybeDelay()
    {
        if (SimulatedDelayMs > 0)
            await Task.Delay(SimulatedDelayMs);
    }

    private void ThrowIfFailure()
    {
        if (SimulateFailure)
            throw new HttpRequestException("Simulated SPARK API failure");
    }
}
