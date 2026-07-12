namespace SamorodinkaTech.Fiducia.Domain.Models.TrueConf;

/// <summary>
/// Пользователь TrueConf Server.
/// </summary>
public class TrueConfUser
{
    /// <summary>Логин пользователя (login_name).</summary>
    public string LoginName { get; init; } = string.Empty;

    /// <summary>Отображаемое имя (display_name).</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Email пользователя (email).</summary>
    public string? Email { get; init; }
}
