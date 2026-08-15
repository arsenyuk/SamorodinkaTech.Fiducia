using SamorodinkaTech.Fiducia.Domain.Models.CbrFinOrg;

namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Клиент для взаимодействия с SOAP-сервисом ЦБ РФ (FinOrg.asmx).
/// Предоставляет операции поиска и получения информации об участниках финансового рынка.
/// </summary>
public interface ICbrFinOrgClient
{
    /// <summary>
    /// Возвращает полную информацию об организации по ИНН.
    /// </summary>
    /// <param name="inn">ИНН организации (10 знаков для ЮЛ, 12 для ИП).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Полная информация об организации или null, если не найдена.</returns>
    Task<CbrFinOrgOrganization?> GetOrganizationByInnAsync(
        long inn,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает полную информацию об организации по ОГРН.
    /// </summary>
    /// <param name="ogrn">ОГРН организации.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Полная информация об организации или null, если не найдена.</returns>
    Task<CbrFinOrgOrganization?> GetOrganizationByOgrnAsync(
        long ogrn,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Поиск организаций по наименованию и/или адресу с пагинацией.
    /// </summary>
    /// <param name="name">Наименование организации (частичное совпадение).</param>
    /// <param name="address">Адрес организации (частичное совпадение).</param>
    /// <param name="page">Номер страницы (начиная с 0).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат поиска с пагинацией.</returns>
    Task<CbrFinOrgSearchResult> SearchAsync(
        string? name,
        string? address,
        int page = 0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Массовый поиск организаций по массиву ИНН.
    /// </summary>
    /// <param name="inns">Массив ИНН для поиска.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Список найденных кратких записей.</returns>
    Task<List<CbrFinOrgRecord>> SearchByInnsAsync(
        long[] inns,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает дату последнего обновления данных в справочнике ЦБ РФ.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Дата и время последнего обновления.</returns>
    Task<DateTime> GetLastUpdateAsync(
        CancellationToken cancellationToken = default);
}
