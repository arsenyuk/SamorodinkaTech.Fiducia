namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Stream Telecom SMS platform settings (ADR-022).
/// All values come from configuration file.
/// Secrets (Login, Password) must be stored in .env, not in appsettings.json.
/// </summary>
public class StreamTelecomOptions
{
    /// <summary>Base URL of Stream Telecom REST API.</summary>
    public string BaseUrl { get; init; } = "https://gateway.api.sc/rest";

    /// <summary>Login for Stream Telecom account. Empty string = integration disabled.</summary>
    public string Login { get; init; } = "";

    /// <summary>API password. Empty string = integration disabled.</summary>
    public string Password { get; init; } = "";

    /// <summary>Default sender name (registered on the platform).</summary>
    public string DefaultSender { get; init; } = "";

    /// <summary>Enable/disable flag. false = client not registered in DI.</summary>
    public bool Enabled { get; init; }
}
