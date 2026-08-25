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
    private readonly Dictionary<string, SparkEntrepreneur> _entrepreneurs = new();
    private readonly Dictionary<string, SparkCompanyExtended> _extendedCompanies = new();
    private readonly Dictionary<string, SparkCompanyStructure> _structures = new();
    private readonly Dictionary<string, SparkCoownersHistory> _coownersHistory = new();
    private readonly Dictionary<string, SparkPersonCompliance> _personCompliance = new();
    private SparkStateAccount? _stateAccount;

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

    /// <inheritdoc />
    public async Task<SparkEntrepreneur?> GetEntrepreneurByInnAsync(
        string inn,
        CancellationToken cancellationToken = default)
    {
        await MaybeDelay(cancellationToken);
        ThrowIfFailure();

        _entrepreneurs.TryGetValue(inn, out var entrepreneur);
        return entrepreneur;
    }

    /// <inheritdoc />
    public async Task<SparkCompanyExtended?> GetCompanyExtendedAsync(
        string inn,
        CancellationToken cancellationToken = default)
    {
        await MaybeDelay(cancellationToken);
        ThrowIfFailure();

        _extendedCompanies.TryGetValue(inn, out var company);
        return company;
    }

    /// <inheritdoc />
    public async Task<SparkCompanyStructure?> GetCompanyStructureAsync(
        string inn,
        CancellationToken cancellationToken = default)
    {
        await MaybeDelay(cancellationToken);
        ThrowIfFailure();

        _structures.TryGetValue(inn, out var structure);
        return structure;
    }

    /// <inheritdoc />
    public async Task<SparkStateAccount?> GetStateAccountAsync(
        CancellationToken cancellationToken = default)
    {
        await MaybeDelay(cancellationToken);
        ThrowIfFailure();

        return _stateAccount;
    }

    /// <inheritdoc />
    public async Task<SparkCoownersHistory?> GetCoownersHistoryAsync(
        string inn,
        CancellationToken cancellationToken = default)
    {
        await MaybeDelay(cancellationToken);
        ThrowIfFailure();

        _coownersHistory.TryGetValue(inn, out var history);
        return history;
    }

    /// <inheritdoc />
    public async Task<SparkPersonCompliance?> GetPersonComplianceAsync(
        string inn,
        CancellationToken cancellationToken = default)
    {
        await MaybeDelay(cancellationToken);
        ThrowIfFailure();

        _personCompliance.TryGetValue(inn, out var compliance);
        return compliance;
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

    /// <summary>
    /// Добавляет данные ИП в mock-хранилище.
    /// </summary>
    public void AddEntrepreneur(string inn, SparkEntrepreneur entrepreneur)
    {
        _entrepreneurs[inn] = entrepreneur;
    }

    /// <summary>
    /// Добавляет расширенную карточку компании в mock-хранилище.
    /// </summary>
    public void AddExtendedCompany(string inn, SparkCompanyExtended company)
    {
        _extendedCompanies[inn] = company;
    }

    /// <summary>
    /// Добавляет структуру компании в mock-хранилище.
    /// </summary>
    public void AddCompanyStructure(string inn, SparkCompanyStructure structure)
    {
        _structures[inn] = structure;
    }

    /// <summary>
    /// Устанавливает данные аккаунта СПАРК.
    /// </summary>
    public void SetStateAccount(SparkStateAccount account)
    {
        _stateAccount = account;
    }

    /// <summary>
    /// Добавляет историю совладельцев в mock-хранилище.
    /// </summary>
    public void AddCoownersHistory(string inn, SparkCoownersHistory history)
    {
        _coownersHistory[inn] = history;
    }

    /// <summary>
    /// Добавляет отчёт о соответствии в mock-хранилище.
    /// </summary>
    public void AddPersonCompliance(string inn, SparkPersonCompliance compliance)
    {
        _personCompliance[inn] = compliance;
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
