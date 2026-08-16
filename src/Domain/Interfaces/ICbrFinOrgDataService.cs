using SamorodinkaTech.Fiducia.Domain.Models.CbrFinOrg;

namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Сервис кэширования данных ЦБ РФ (FinOrg) в БД.
/// TTL кэша: 24 часа. При устаревании — автоматический рефреш через API.
/// </summary>
public interface ICbrFinOrgDataService
{
    /// <summary>
    /// Возвращает информацию об организации по ИНН.
    /// Если кэш актуален (fetched_at младше 24ч) — читает из БД.
    /// Если кэш устарел или отсутствует — вызывает API ЦБ, сохраняет в БД, возвращает.
    /// </summary>
    /// <param name="inn">ИНН организации.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Полная информация об организации или null, если не найдена в ЦБ.</returns>
    Task<CbrFinOrgOrganization?> GetOrganizationByInnAsync(
        long inn,
        CancellationToken cancellationToken = default);
}
