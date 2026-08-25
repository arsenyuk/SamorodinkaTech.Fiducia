using SamorodinkaTech.Fiducia.Domain.Models.Spark;

namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Клиент для взаимодействия с СПАРК API (Интерфакс).
/// Предоставляет операции поиска компании по ИНН, получения карточки компании
/// и данных о генеральном директоре.
/// </summary>
public interface ISparkApiClient
{
    /// <summary>
    /// Возвращает карточку компании по ИНН (краткая справка).
    /// </summary>
    Task<SparkCompany?> GetCompanyByInnAsync(
        string inn,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает данные о генеральном директоре компании.
    /// </summary>
    Task<SparkManager?> GetManagerAsync(
        string inn,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает список учредителей (участников) компании.
    /// </summary>
    Task<List<SparkFounder>> GetFoundersAsync(
        string inn,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает краткую справку по ИП (GetEnterpreneurShortReport).
    /// </summary>
    Task<SparkEntrepreneur?> GetEntrepreneurByInnAsync(
        string inn,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает расширенную карточку компании (GetCompanyExtendedReport).
    /// </summary>
    Task<SparkCompanyExtended?> GetCompanyExtendedAsync(
        string inn,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает структуру компании (GetCompanyStructure).
    /// Материнская → текущая → дочерние + аффилированные лица.
    /// </summary>
    Task<SparkCompanyStructure?> GetCompanyStructureAsync(
        string inn,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает информацию об аккаунте СПАРК (GetStateAccount).
    /// Остаток лимита платных запросов.
    /// </summary>
    Task<SparkStateAccount?> GetStateAccountAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает историю совладельцев компании (GetCompanyCoownersHistory).
    /// Текущие + бывшие совладельцы с датами.
    /// </summary>
    Task<SparkCoownersHistory?> GetCoownersHistoryAsync(
        string inn,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает отчёт о соответствии (GetPersonComplianceReport).
    /// Проверка санкционных рисков, связей с ПДЛ/ПЭП.
    /// </summary>
    Task<SparkPersonCompliance?> GetPersonComplianceAsync(
        string inn,
        CancellationToken cancellationToken = default);
}
