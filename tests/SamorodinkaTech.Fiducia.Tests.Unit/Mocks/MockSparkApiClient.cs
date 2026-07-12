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
    private readonly Dictionary<string, List<SparkFounder>> _founders = new();

    /// <summary>Задержка ответа в миллисекундах для имитации сети (по умолчанию 0).</summary>
    public int SimulatedDelayMs { get; set; }

    /// <summary>Если true, все методы выбрасывают исключение (имитация сбоя API).</summary>
    public bool SimulateFailure { get; set; }

    /// <summary>Если true, GetFoundersAsync возвращает 403 Forbidden (недостаточная лицензия).</summary>
    public bool SimulateForbiddenOnFounders { get; set; }

    /// <inheritdoc />
    public async Task<SparkCompany?> GetCompanyByInnAsync(
        string inn,
        CancellationToken cancellationToken = default)
    {
        await MaybeDelay(cancellationToken);
        ThrowIfFailure();

        _companies.TryGetValue(inn, out var company);
        return company;
    }

    /// <inheritdoc />
    public async Task<SparkManager?> GetManagerAsync(
        string inn,
        CancellationToken cancellationToken = default)
    {
        await MaybeDelay(cancellationToken);
        ThrowIfFailure();

        _managers.TryGetValue(inn, out var manager);
        return manager;
    }

    /// <inheritdoc />
    public async Task<List<SparkFounder>> GetFoundersAsync(
        string inn,
        CancellationToken cancellationToken = default)
    {
        await MaybeDelay(cancellationToken);
        ThrowIfFailure();

        if (SimulateForbiddenOnFounders)
            throw new HttpRequestException("Response status code does not indicate success: 403 (Forbidden).",
                null, System.Net.HttpStatusCode.Forbidden);

        _founders.TryGetValue(inn, out var founders);
        return founders ?? new List<SparkFounder>();
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

    /// <summary>
    /// Добавляет список учредителей в mock-хранилище (для настройки тестовых данных).
    /// </summary>
    public void AddFounders(string inn, List<SparkFounder> founders)
    {
        _founders[inn] = founders;
    }

    private async Task MaybeDelay(CancellationToken cancellationToken = default)
    {
        if (SimulatedDelayMs > 0)
            await Task.Delay(SimulatedDelayMs, cancellationToken);
    }

    private void ThrowIfFailure()
    {
        if (SimulateFailure)
            throw new HttpRequestException("Simulated SPARK API failure");
    }
}
