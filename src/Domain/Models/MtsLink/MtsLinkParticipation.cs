namespace SamorodinkaTech.Fiducia.Domain.Models.MtsLink;

/// <summary>
/// Ответ регистрации участника на мероприятие МТС Линк.
/// </summary>
public class MtsLinkParticipation
{
    /// <summary>Уникальный идентификатор участника в мероприятии (participationId).</summary>
    public int ParticipationId { get; init; }

    /// <summary>Персональная ссылка для входа (link).</summary>
    public string Link { get; init; } = string.Empty;

    /// <summary>Идентификатор контакта (contactId).</summary>
    public int? ContactId { get; init; }
}
