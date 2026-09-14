namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// DaData integration settings (ADR-022).
/// All values come from configuration file.
/// Secrets (ApiKey, SecretKey) must be stored in .env, not in appsettings.json.
/// </summary>
public class DaDataOptions
{
    /// <summary>Base URL of DaData API.</summary>
    public string BaseUrl { get; init; } = "https://api.dadata.ru/v2";

    /// <summary>API token for authentication. Empty string = integration disabled.</summary>
    public string ApiKey { get; init; } = "";

    /// <summary>Secret key for DaData API. Empty string = integration disabled.</summary>
    public string SecretKey { get; init; } = "";

    /// <summary>Enable/disable flag. false = client not registered in DI.</summary>
    public bool Enabled { get; init; }
}
