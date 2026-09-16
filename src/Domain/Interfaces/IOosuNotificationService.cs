namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Сервис отправки уведомлений ООСУ без loopback HTTP.
/// Заменяет POST /send эндпоинт OosuNotificationEndpoints.
/// </summary>
public interface IOosuNotificationService
{
    /// <summary>
    /// Отправляет уведомления ООСУ участникам: обновляет данные собрания,
    /// формирует DOCX для каждого участника, отправляет уведомления в системе.
    /// </summary>
    /// <param name="model">Данные для отправки.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Список идентификаторов созданных файлов.</returns>
    Task<List<Guid>> SendAsync(OosuNotificationSendModel model, CancellationToken ct = default);
}

/// <summary>
/// Модель для отправки уведомлений ООСУ.
/// </summary>
public class OosuNotificationSendModel
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

    /// <summary>Место ознакомления с документами.</summary>
    public string? ReviewLocation { get; init; }

    /// <summary>ФИО Генерального директора (подпись).</summary>
    public string CeoName { get; init; } = default!;

    /// <summary>Финансовый год.</summary>
    public int? FinancialYear { get; init; }
}
