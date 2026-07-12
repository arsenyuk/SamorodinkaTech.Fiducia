namespace SamorodinkaTech.Fiducia.Domain.Models.MtsLink;

/// <summary>
/// Ответ создания шаблона мероприятия (Event) в МТС Линк.
/// </summary>
public class MtsLinkEvent
{
    /// <summary>Идентификатор шаблона (eventId).</summary>
    public int EventId { get; init; }

    /// <summary>Публичная ссылка на лендинг мероприятия (link).</summary>
    public string Link { get; init; } = string.Empty;
}
