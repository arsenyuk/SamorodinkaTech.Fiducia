using SamorodinkaTech.Fiducia.Domain.Models.TrueConf;

namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Клиент для взаимодействия с TrueConf Server API v4.
/// Предоставляет операции управления конференциями и пользователями
/// для интеграции с заседаниями совета директоров.
/// </summary>
public interface ITrueConfApiClient
{
    /// <summary>
    /// Получает OAuth2-токен доступа к API TrueConf Server.
    /// </summary>
    /// <param name="clientId">Идентификатор OAuth2-приложения.</param>
    /// <param name="clientSecret">Секретный ключ OAuth2-приложения.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Токен доступа.</returns>
    Task<TrueConfTokenResponse> GetTokenAsync(
        string clientId,
        string clientSecret,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Создаёт конференцию для заседания совета директоров.
    /// </summary>
    /// <param name="request">Параметры конференции.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Созданная конференция.</returns>
    Task<TrueConfConference> CreateConferenceAsync(
        CreateTrueConfConferenceRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает информацию о конференции по идентификатору.
    /// </summary>
    /// <param name="conferenceId">Идентификатор конференции.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Данные конференции или null, если не найдена.</returns>
    Task<TrueConfConference?> GetConferenceAsync(
        string conferenceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Удаляет конференцию по идентификатору.
    /// </summary>
    /// <param name="conferenceId">Идентификатор конференции.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>true, если конференция удалена.</returns>
    Task<bool> DeleteConferenceAsync(
        string conferenceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает список завершённых конференций с указанным тегом.
    /// </summary>
    /// <param name="tag">Тег для фильтрации.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task<IReadOnlyList<TrueConfConference>> GetStoppedConferencesAsync(
        string? tag = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает список пользователей сервера TrueConf.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task<IReadOnlyList<TrueConfUser>> GetUsersAsync(
        CancellationToken cancellationToken = default);
}
