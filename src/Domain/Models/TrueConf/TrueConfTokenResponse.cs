namespace SamorodinkaTech.Fiducia.Domain.Models.TrueConf;

/// <summary>
/// Ответ OAuth2-авторизации TrueConf Server API.
/// </summary>
public class TrueConfTokenResponse
{
    /// <summary>Токен доступа (access_token).</summary>
    public string AccessToken { get; init; } = string.Empty;
}
