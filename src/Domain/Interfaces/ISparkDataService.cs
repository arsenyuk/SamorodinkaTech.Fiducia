using SamorodinkaTech.Fiducia.Domain.Models.Spark;

namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Кэшированные данные СПАРК для отображения на форме.
/// </summary>
public record SparkCachedView
{
    public string? ManagerName { get; init; }
    public string? ManagerPosition { get; init; }
    public string? ManagerInn { get; init; }
    public DateTime? ManagerStartDate { get; init; }
    public string? CompanyFullName { get; init; }
    public string? CompanyShortName { get; init; }
    public string? CompanyOgrn { get; init; }
    public string? CompanyOkopfName { get; init; }
    public string? CompanyAddress { get; init; }
    public string? CompanyStatus { get; init; }
    public DateTime? CompanyRegDate { get; init; }
    public List<SparkFounder> Founders { get; init; } = new();
}

/// <summary>
/// Результат загрузки свежих данных из СПАРК API.
/// </summary>
public record SparkFetchResult
{
    public SparkCompany? Company { get; init; }
    public SparkManager? Manager { get; init; }
    public List<SparkFounder> Founders { get; init; } = new();
    public string? Warning { get; init; }
    /// <summary>Нормализованный код ОКОПФ из СПАРК (только цифры), если найден.</summary>
    public string? OkopfCode { get; init; }
}

/// <summary>
/// Сервис для загрузки и кэширования данных СПАРК.
/// Отвечает за вызовы API, сохранение в ext_spark_* и чтение кэша.
/// </summary>
public interface ISparkDataService
{
    /// <summary>
    /// Загружает кэшированные данные СПАРК из БД (ext_spark_* таблицы).
    /// </summary>
    Task<SparkCachedView> LoadCachedAsync(
        string inn,
        bool isLlc,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Запрашивает свежие данные из СПАРК API, сохраняет в ext_spark_* таблицы,
    /// обновляет ОКОПФ юридического лица (если найден).
    /// </summary>
    /// <returns>Результат загрузки и нормализованный код ОКОПФ.</returns>
    Task<SparkFetchResult> FetchAndSaveAsync(
        string inn,
        bool isLlc,
        Guid legalEntityId,
        CancellationToken cancellationToken = default);
}