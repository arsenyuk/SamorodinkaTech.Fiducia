using SamorodinkaTech.Fiducia.Domain.Models.MtsLink;

namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Клиент для взаимодействия с API МТС Линк (v3).
/// Предоставляет операции создания встреч, регистрации участников
/// и управления мероприятиями для заседаний совета директоров.
/// </summary>
public interface IMtsLinkApiClient
{
    /// <summary>
    /// Создаёт встречу для заседания совета директоров.
    /// Выполняет двухшаговое создание: Event (шаблон) → EventSession (встреча).
    /// </summary>
    /// <param name="request">Параметры встречи.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Созданная встреча (EventSession).</returns>
    Task<MtsLinkEventSession> CreateMeetingAsync(
        CreateMtsLinkMeetingRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает информацию о мероприятии по идентификатору.
    /// </summary>
    /// <param name="eventSessionId">Идентификатор мероприятия (EventSession).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Данные мероприятия или null, если не найдено.</returns>
    Task<MtsLinkEventSession?> GetEventSessionAsync(
        int eventSessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Удаляет мероприятие по идентификатору.
    /// </summary>
    /// <param name="eventSessionId">Идентификатор мероприятия.</param>
    /// <param name="sendEmail">Отправлять ли участникам письмо об отмене.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>true, если мероприятие удалено.</returns>
    Task<bool> DeleteEventSessionAsync(
        int eventSessionId,
        bool sendEmail = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Запускает мероприятие (начинает трансляцию).
    /// </summary>
    /// <param name="eventSessionId">Идентификатор мероприятия.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task StartEventSessionAsync(
        int eventSessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Завершает мероприятие (останавливает трансляцию).
    /// </summary>
    /// <param name="eventSessionId">Идентификатор мероприятия.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task StopEventSessionAsync(
        int eventSessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Регистрирует участника на мероприятие.
    /// </summary>
    /// <param name="eventSessionId">Идентификатор мероприятия.</param>
    /// <param name="request">Данные участника.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Информация об участии с персональной ссылкой.</returns>
    Task<MtsLinkParticipation> RegisterParticipantAsync(
        int eventSessionId,
        RegisterMtsLinkParticipantRequest request,
        CancellationToken cancellationToken = default);
}
