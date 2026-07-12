namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Настройки интеграции с TrueConf Server (ADR-022).
/// Все значения — из конфигурационного файла.
/// </summary>
public class TrueConfOptions
{
    /// <summary>Базовый URL TrueConf Server API.</summary>
    public string BaseUrl { get; init; } = "";

    /// <summary>OAuth2 Client ID (пустая строка — интеграция отключена).</summary>
    public string ClientId { get; init; } = "";

    /// <summary>OAuth2 Client Secret.</summary>
    public string ClientSecret { get; init; } = "";

    /// <summary>Флаг включения интеграции.</summary>
    public bool Enabled { get; init; }
}
