namespace SamorodinkaTech.Fiducia.Domain.Models.MtsLink;

/// <summary>
/// Запрос на регистрацию участника в мероприятии МТС Линк.
/// </summary>
public class RegisterMtsLinkParticipantRequest
{
    /// <summary>Email участника (обязательно).</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>Имя участника.</summary>
    public string? Name { get; init; }

    /// <summary>Фамилия участника.</summary>
    public string? SecondName { get; init; }

    /// <summary>Роль: GUEST / LECTURER / ADMIN.</summary>
    public string Role { get; init; } = "GUEST";

    /// <summary>Отправлять ли email о регистрации.</summary>
    public bool SendEmail { get; init; }
}
