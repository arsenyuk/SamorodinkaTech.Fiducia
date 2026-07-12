namespace SamorodinkaTech.Fiducia.Domain.Models.MtsLink;

/// <summary>
/// Запрос на регистрацию участника на сессию MTS Link.
/// </summary>
public class RegisterMtsLinkParticipantRequest
{
    /// <summary>Email участника (обязательно).</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>Имя участника.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Фамилия участника.</summary>
    public string SecondName { get; init; } = string.Empty;

    /// <summary>Роль участника (GUEST — обычный участник).</summary>
    public string Role { get; init; } = "GUEST";

    /// <summary>Автоматический вход в комнату при старте.</summary>
    public bool IsAutoEnter { get; init; } = true;

    /// <summary>Отправить письмо-приглашение на email.</summary>
    public bool SendEmail { get; init; } = true;
}
