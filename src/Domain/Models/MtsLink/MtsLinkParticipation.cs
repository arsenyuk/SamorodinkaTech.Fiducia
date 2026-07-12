namespace SamorodinkaTech.Fiducia.Domain.Models.MtsLink;

/// <summary>
/// Регистрация участника на сессию MTS Link.
/// </summary>
public class MtsLinkParticipation
{
    /// <summary>Идентификатор регистрации (participationId).</summary>
    public int ParticipationId { get; init; }

    /// <summary>Персональная ссылка для входа в сессию (link).</summary>
    public string Link { get; init; } = string.Empty;

    /// <summary>Идентификатор контакта в адресной книге MTS Link (contactId).</summary>
    public int? ContactId { get; init; }
}
