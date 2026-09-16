namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Сервис записи данных запросов участника в общество (share_request).
/// Заменяет POST/PUT/DELETE loopback HTTP-вызовы из Blazor-страниц к /api/share-requests.
/// </summary>
public interface IShareRequestWriteService
{
    /// <summary>Создать одиночный запрос.</summary>
    /// <param name="userId">Идентификатор пользователя-создателя.</param>
    /// <param name="requestTypeId">Идентификатор типа запроса (ref_request_type).</param>
    /// <param name="payload">JSON-данные запроса.</param>
    /// <returns>Идентификатор созданного запроса.</returns>
    Task<Guid> CreateAsync(Guid userId, Guid requestTypeId, string? payload, CancellationToken ct = default);

    /// <summary>Обновить payload черновика.</summary>
    /// <param name="id">Идентификатор запроса.</param>
    /// <param name="payload">Новые JSON-данные.</param>
    Task UpdatePayloadAsync(Guid id, string payload, CancellationToken ct = default);

    /// <summary>Отправить запрос (перевод draft → submitted).</summary>
    /// <param name="id">Идентификатор запроса.</param>
    Task SubmitAsync(Guid id, CancellationToken ct = default);

    /// <summary>Отозвать запрос (только NOTARIAL_OFFER, в течение 24ч).</summary>
    /// <param name="id">Идентификатор запроса.</param>
    /// <param name="notarized">Отзыв с нотариальным заверением.</param>
    Task RevokeAsync(Guid id, bool notarized, CancellationToken ct = default);

    /// <summary>Поддержать коллективное требование.</summary>
    /// <param name="id">Идентификатор запроса.</param>
    /// <param name="participantId">Идентификатор участника СД, оказывающего поддержку.</param>
    Task SupportAsync(Guid id, Guid participantId, CancellationToken ct = default);

    /// <summary>Отозвать поддержку коллективного требования.</summary>
    /// <param name="id">Идентификатор запроса.</param>
    /// <param name="participantId">Идентификатор участника СД, отзывающего поддержку.</param>
    Task WithdrawAsync(Guid id, Guid participantId, CancellationToken ct = default);

    /// <summary>Решение ГД или СД по требованию.</summary>
    /// <param name="id">Идентификатор запроса.</param>
    /// <param name="approved">true — принято, false — отклонено.</param>
    /// <param name="reason">Комментарий к решению.</param>
    Task DecideAsync(Guid id, bool approved, string? reason, CancellationToken ct = default);

    /// <summary>Создать коллективное требование с автоматическим добавлением поддержки инициатора.</summary>
    /// <param name="userId">Идентификатор пользователя-инициатора.</param>
    /// <param name="requestTypeId">Идентификатор типа требования.</param>
    /// <param name="payload">JSON-данные требования.</param>
    /// <returns>Идентификатор созданного запроса.</returns>
    Task<Guid> CreateCollectiveAsync(Guid userId, Guid requestTypeId, string? payload, CancellationToken ct = default);

    /// <summary>Добавить пункт в структурированное требование.</summary>
    /// <param name="requestId">Идентификатор запроса.</param>
    /// <param name="title">Заголовок пункта.</param>
    /// <param name="description">Описание пункта.</param>
    /// <returns>Идентификатор созданного пункта.</returns>
    Task<Guid> AddItemAsync(Guid requestId, string title, string? description, CancellationToken ct = default);

    /// <summary>Прикрепить файл к пункту требования.</summary>
    /// <param name="requestId">Идентификатор запроса.</param>
    /// <param name="itemId">Идентификатор пункта.</param>
    /// <param name="fileId">Идентификатор файла (files).</param>
    Task AttachFileToItemAsync(Guid requestId, Guid itemId, Guid fileId, CancellationToken ct = default);

    /// <summary>Прикрепить файл к требованию.</summary>
    /// <param name="requestId">Идентификатор запроса.</param>
    /// <param name="fileId">Идентификатор файла (files).</param>
    Task AttachFileAsync(Guid requestId, Guid fileId, CancellationToken ct = default);

    /// <summary>Сформировать уведомления ВОСУ участникам по требованию DEMAND_VOSU.</summary>
    /// <param name="requestId">Идентификатор запроса.</param>
    /// <returns>Количество сформированных уведомлений.</returns>
    Task<int> GenerateVosuNotificationsAsync(Guid requestId, CancellationToken ct = default);
}
