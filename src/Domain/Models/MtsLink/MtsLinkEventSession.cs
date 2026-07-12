namespace SamorodinkaTech.Fiducia.Domain.Models.MtsLink;

/// <summary>
/// Сессия мероприятия MTS Link — конкретное проведение мероприятия.
/// </summary>
public class MtsLinkEventSession
{
    /// <summary>Идентификатор сессии (id).</summary>
    public int Id { get; init; }

    /// <summary>Название мероприятия (name).</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Статус сессии: ACTIVE / STOP (status).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Дата и время начала в ISO 8601 (startsAt).</summary>
    public string StartsAt { get; init; } = string.Empty;

    /// <summary>Публичная ссылка на сессию (link).</summary>
    public string Link { get; init; } = string.Empty;

    /// <summary>Тип мероприятия: meeting / webinar / training (type).</summary>
    public string Type { get; init; } = string.Empty;
}
