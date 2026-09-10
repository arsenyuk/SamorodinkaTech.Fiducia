namespace SamorodinkaTech.Fiducia.Domain.Models;

/// <summary>
/// Данные для формирования уведомления участника о созыве/изменении повестки ВОСУ.
/// </summary>
public class VosuNotificationData
{
    /// <summary>Полное наименование юридического лица.</summary>
    public string LegalEntityName { get; init; } = default!;

    /// <summary>ОГРН юридического лица.</summary>
    public string? LegalEntityOgrn { get; init; }

    /// <summary>ИНН юридического лица.</summary>
    public string? LegalEntityInn { get; init; }

    /// <summary>ФИО участника (Фамилия Имя Отчество).</summary>
    public string ParticipantFullName { get; init; } = default!;

    /// <summary>Адрес участника для корреспонденции.</summary>
    public string? ParticipantAddress { get; init; }

    /// <summary>Дата проведения собрания.</summary>
    public DateOnly MeetingDate { get; init; }

    /// <summary>Время начала собрания.</summary>
    public TimeOnly? MeetingStartTime { get; init; }

    /// <summary>Место проведения собрания.</summary>
    public string? MeetingVenue { get; init; }

    /// <summary>Время начала регистрации участников.</summary>
    public TimeOnly? RegistrationStartTime { get; init; }

    /// <summary>Повестка дня (список вопросов).</summary>
    public List<string> AgendaItems { get; init; } = new();

    /// <summary>ФИО инициатора (подавшего требование).</summary>
    public string? InitiatorName { get; init; }

    /// <summary>Доля инициатора в уставном капитале (%).</summary>
    public decimal? InitiatorSharePercent { get; init; }

    /// <summary>Дата получения требования ГД.</summary>
    public DateOnly? DemandReceivedDate { get; init; }

    /// <summary>Место ознакомления с документами.</summary>
    public string? ReviewLocation { get; init; }

    /// <summary>ФИО Генерального директора (подпись).</summary>
    public string CeoName { get; init; } = default!;

    /// <summary>Дата формирования уведомления.</summary>
    public DateOnly NotificationDate { get; init; }

    /// <summary>Признак: изменение повестки (true) или первоначальное уведомление (false).</summary>
    public bool IsAgendaChange { get; init; }
}
