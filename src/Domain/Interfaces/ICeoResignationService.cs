namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Сервис уведомления ГД об увольнении (ст. 280 ТК РФ) без loopback HTTP.
/// Заменяет POST /send эндпоинт CeoResignationEndpoints.
/// </summary>
public interface ICeoResignationService
{
    /// <summary>
    /// Отправляет уведомление об увольнении ГД: формирует DOCX для каждого участника,
    /// отправляет уведомления в системе, создаёт требование ВОСУ.
    /// </summary>
    /// <param name="model">Данные для отправки.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Результат отправки.</returns>
    Task<CeoResignationResult> SendAsync(CeoResignationSendModel model, CancellationToken ct = default);
}

/// <summary>
/// Модель для отправки уведомления ГД об увольнении.
/// </summary>
public class CeoResignationSendModel
{
    /// <summary>Идентификатор юридического лица.</summary>
    public Guid LegalEntityId { get; init; }

    /// <summary>Идентификатор текущего пользователя (ГД).</summary>
    public Guid UserId { get; init; }

    /// <summary>Плановая дата увольнения (не ранее чем через 30 дней).</summary>
    public DateOnly ResignationDate { get; init; }

    /// <summary>Место ознакомления с документами.</summary>
    public string? ReviewLocation { get; init; }

    /// <summary>Повестка ВОСУ (по умолчанию: избрание нового ГД).</summary>
    public List<string>? AgendaItems { get; init; }
}

/// <summary>
/// Результат отправки уведомления ГД об увольнении.
/// </summary>
public class CeoResignationResult
{
    /// <summary>Количество отправленных уведомлений.</summary>
    public int SentCount { get; init; }

    /// <summary>Количество созданных DOCX-файлов.</summary>
    public int FileCount { get; init; }

    /// <summary>Плановая дата увольнения.</summary>
    public DateOnly ResignationDate { get; init; }

    /// <summary>Количество дней до даты увольнения.</summary>
    public int DaysUntilResignation { get; init; }
}
