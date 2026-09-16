namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Сервис отправки уведомлений ВОСУ без loopback HTTP.
/// Заменяет POST /send эндпоинт VosuNotificationEndpoints.
/// </summary>
public interface IVosuNotificationService
{
    /// <summary>
    /// Отправляет уведомления ВОСУ участникам: обновляет данные собрания,
    /// формирует DOCX для каждого участника, отправляет уведомления в системе.
    /// </summary>
    /// <param name="model">Данные для отправки.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Список идентификаторов созданных файлов.</returns>
    Task<List<Guid>> SendAsync(VosuNotificationSendModel model, CancellationToken ct = default);
}

/// <summary>
/// Модель для отправки уведомлений ВОСУ.
/// </summary>
public class VosuNotificationSendModel
{
    /// <summary>Идентификатор собрания.</summary>
    public Guid MeetingId { get; init; }

    /// <summary>Идентификатор юридического лица.</summary>
    public Guid LegalEntityId { get; init; }

    /// <summary>Идентификатор текущего пользователя (ГД).</summary>
    public Guid UserId { get; init; }

    /// <summary>Дата проведения собрания.</summary>
    public DateOnly MeetingDate { get; init; }

    /// <summary>Время начала собрания.</summary>
    public TimeOnly? MeetingStartTime { get; init; }

    /// <summary>Место проведения собрания.</summary>
    public string? MeetingVenue { get; init; }

    /// <summary>Время начала регистрации участников.</summary>
    public TimeOnly? RegistrationStartTime { get; init; }

    /// <summary>Вопросы повестки.</summary>
    public List<string>? AgendaItems { get; init; }

    /// <summary>ФИО инициатора (подавшего требование).</summary>
    public string? InitiatorName { get; init; }

    /// <summary>Доля инициатора в уставном капитале (%).</summary>
    public decimal? InitiatorSharePercent { get; init; }

    /// <summary>Дата получения требования ГД.</summary>
    public DateOnly? DemandReceivedDate { get; init; }

    /// <summary>Место ознакомления с документами.</summary>
    public string? ReviewLocation { get; init; }

    /// <summary>Признак: изменение повестки (true) или первоначальное уведомление (false).</summary>
    public bool IsAgendaChange { get; init; }

    /// <summary>ФИО Генерального директора (подпись).</summary>
    public string CeoName { get; init; } = default!;
}
