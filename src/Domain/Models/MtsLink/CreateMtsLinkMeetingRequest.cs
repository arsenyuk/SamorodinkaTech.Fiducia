namespace SamorodinkaTech.Fiducia.Domain.Models.MtsLink;

/// <summary>
/// Запрос на создание мероприятия в МТС Линк для заседания совета директоров.
/// </summary>
public class CreateMtsLinkMeetingRequest
{
    /// <summary>Название мероприятия.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Дата и время начала в формате '2025-06-09T20:15:00+03:00'.</summary>
    public string StartsAtTimestamp { get; init; } = string.Empty;

    /// <summary>Тип мероприятия: meeting / webinar / training.</summary>
    public string Type { get; init; } = "meeting";

    /// <summary>Длительность в формате ISO 8601 (PT1H30M0S).</summary>
    public string Duration { get; init; } = "PT1H30M0S";

    /// <summary>Описание мероприятия.</summary>
    public string? Description { get; init; }

    /// <summary>Теги мероприятия.</summary>
    public string[]? Tags { get; init; }

    /// <summary>Язык интерфейса: RU / EN.</summary>
    public string Lang { get; init; } = "RU";
}
