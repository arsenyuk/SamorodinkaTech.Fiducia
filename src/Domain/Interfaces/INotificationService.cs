using SamorodinkaTech.Fiducia.Domain.Enums;

namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Сервис отправки уведомлений участникам совета директоров.
/// Отправка по внешним каналам связи (Email/SMS/Push) не выполняется —
/// сервис фиксирует факт уведомления в таблице notifications и в логе приложения (РСБ.3).
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Формирует и фиксирует отправку уведомления одному пользователю.
    /// </summary>
    /// <param name="notificationType">Тип уведомления.</param>
    /// <param name="title">Заголовок уведомления.</param>
    /// <param name="body">Текст уведомления.</param>
    /// <param name="userId">Идентификатор получателя (директор/член комитета).</param>
    /// <param name="committeeId">Идентификатор комитета (если уведомление связано с комитетом).</param>
    /// <param name="meetingId">Идентификатор заседания (если уведомление связано с заседанием).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Идентификатор созданной записи уведомления.</returns>
    Task<int> SendAsync(
        NotificationType notificationType,
        string title,
        string body,
        Guid? userId = null,
        Guid? committeeId = null,
        Guid? meetingId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Формирует и фиксирует отправку одинакового уведомления нескольким пользователям.
    /// </summary>
    /// <param name="notificationType">Тип уведомления.</param>
    /// <param name="title">Заголовок уведомления.</param>
    /// <param name="body">Текст уведомления.</param>
    /// <param name="userIds">Список идентификаторов получателей.</param>
    /// <param name="committeeId">Идентификатор комитета (если уведомление связано с комитетом).</param>
    /// <param name="meetingId">Идентификатор заседания (если уведомление связано с заседанием).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Список идентификаторов созданных записей уведомлений.</returns>
    Task<IReadOnlyList<int>> SendToManyAsync(
        NotificationType notificationType,
        string title,
        string body,
        IEnumerable<Guid> userIds,
        Guid? committeeId = null,
        Guid? meetingId = null,
        CancellationToken cancellationToken = default);
}
