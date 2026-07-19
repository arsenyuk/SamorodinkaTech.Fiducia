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
    /// Возвращает карточку компании по ИНН.
    /// </summary>
    /// <param name="inn">ИНН (10 знаков для ЮЛ).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Карточка компании или null, если не найдена.</returns>
    Task<SparkCompany?> GetCompanyByInnAsync(
        string inn,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает данные о генеральном директоре компании.
    /// </summary>
    /// <param name="inn">ИНН компании.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Данные руководителя или null.</returns>
    Task<SparkManager?> GetManagerAsync(
        string inn,
        CancellationToken cancellationToken = default);
}
