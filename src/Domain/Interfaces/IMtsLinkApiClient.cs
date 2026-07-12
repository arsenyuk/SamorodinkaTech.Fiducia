using SamorodinkaTech.Fiducia.Domain.Models.MtsLink;

namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Клиент для взаимодействия с MTS Link (Webinar.ru) Web API v3.
/// Предоставляет операции управления мероприятиями, сессиями и регистрацией участников
/// для интеграции с заседаниями совета директоров.
/// </summary>
public interface IMtsLinkApiClient
{
    /// <summary>
    /// Создаёт мероприятие и сессию в MTS Link за один вызов (двухшаговое: Event → EventSession).
    /// </summary>
    /// <param name="request">Параметры мероприятия.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Созданная сессия с идентификатором и ссылкой.</returns>
    Task<MtsLinkEventSession> CreateMeetingAsync(
        CreateMtsLinkMeetingRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает информацию о сессии мероприятия по идентификатору.
    /// </summary>
    /// <param name="eventSessionId">Идентификатор сессии.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Данные сессии или null, если не найдена.</returns>
    Task<MtsLinkEventSession?> GetEventSessionAsync(
        int eventSessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Удаляет сессию мероприятия.
    /// </summary>
    /// <param name="eventSessionId">Идентификатор сессии.</param>
    /// <param name="sendEmail">Отправить уведомление участникам об отмене.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>true, если сессия удалена.</returns>
    Task<bool> DeleteEventSessionAsync(
        int eventSessionId,
        bool sendEmail = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Запускает сессию мероприятия.
    /// </summary>
    /// <param name="eventSessionId">Идентификатор сессии.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task StartEventSessionAsync(
        int eventSessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Завершает сессию мероприятия.
    /// </summary>
    /// <param name="eventSessionId">Идентификатор сессии.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task StopEventSessionAsync(
        int eventSessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Регистрирует участника на сессию мероприятия.
    /// </summary>
    /// <param name="eventSessionId">Идентификатор сессии.</param>
    /// <param name="request">Параметры регистрации участника.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Данные регистрации с персональной ссылкой для входа.</returns>
    Task<MtsLinkParticipation> RegisterParticipantAsync(
        int eventSessionId,
        RegisterMtsLinkParticipantRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает список участников зарегистрированных на сессию.
    /// </summary>
    /// <param name="eventSessionId">Идентификатор сессии.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Список зарегистрированных участников.</returns>
    Task<IReadOnlyList<MtsLinkParticipation>> GetParticipationsAsync(
        int eventSessionId,
        CancellationToken cancellationToken = default);
}
