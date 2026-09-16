namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Сервис управления пунктами повестки ОСУ: принятие и отклонение без loopback HTTP.
/// Заменяет POST-эндпоинты AgendaItemEndpoints (accept/reject).
/// </summary>
public interface IAgendaItemService
{
    /// <summary>
    /// Принимает пункт повестки.
    /// </summary>
    /// <param name="id">Идентификатор пункта повестки.</param>
    /// <param name="userId">Идентификатор текущего пользователя.</param>
    /// <param name="legalEntityId">Идентификатор юридического лица.</param>
    /// <param name="ct">Токен отмены.</param>
    Task AcceptAsync(Guid id, Guid userId, Guid legalEntityId, CancellationToken ct = default);

    /// <summary>
    /// Отклоняет пункт повестки.
    /// </summary>
    /// <param name="id">Идентификатор пункта повестки.</param>
    /// <param name="userId">Идентификатор текущего пользователя.</param>
    /// <param name="legalEntityId">Идентификатор юридического лица.</param>
    /// <param name="reason">Причина отклонения (опционально).</param>
    /// <param name="ct">Токен отмены.</param>
    Task RejectAsync(Guid id, Guid userId, Guid legalEntityId, string? reason, CancellationToken ct = default);
}
