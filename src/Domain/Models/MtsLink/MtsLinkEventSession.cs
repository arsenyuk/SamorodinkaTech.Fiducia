namespace SamorodinkaTech.Fiducia.Domain.Models.MtsLink;

/// <summary>
/// Мероприятие (EventSession) в МТС Линк — встреча или вебинар.
/// </summary>
public record MtsLinkEventSession
{
    /// <summary>Идентификатор мероприятия (eventSessionId / id).</summary>
    public int Id { get; init; }

    /// <summary>Название мероприятия (name).</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Состояние мероприятия: ACTIVE / STOP (status).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Дата и время начала (startsAt).</summary>
    public string StartsAt { get; init; } = string.Empty;

    /// <summary>Ссылка для входа (link).</summary>
    public string Link { get; init; } = string.Empty;

    /// <summary>Тип мероприятия: webinar / meeting / training (type).</summary>
    public string Type { get; init; } = string.Empty;
}
