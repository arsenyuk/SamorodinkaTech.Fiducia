namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Настройки интеграции с MTS Link (Webinar.ru) Web API v3 (ADR-022).
/// Все значения — из конфигурационного файла.
/// </summary>
public class MtsLinkOptions
{
    /// <summary>Базовый URL MTS Link API (например, https://userapi.mts-link.ru).</summary>
    public string BaseUrl { get; init; } = "https://userapi.mts-link.ru";

    /// <summary>API-ключ для аутентификации (x-auth-token). Пустая строка — интеграция отключена.</summary>
    public string ApiToken { get; init; } = "";

    /// <summary>Флаг включения интеграции.</summary>
    public bool Enabled { get; init; }
}
