namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Настройки интеграции с СПАРК (Интерфакс).
/// Все значения — из конфигурационного файла (ADR-022).
/// </summary>
public class SparkOptions
{
    /// <summary>Базовый URL API СПАРК.</summary>
    public string BaseUrl { get; init; } = "https://api.spark-interfax.ru";

    /// <summary>API-ключ (пустая строка — интеграция отключена).</summary>
    public string ApiKey { get; init; } = "";
}
