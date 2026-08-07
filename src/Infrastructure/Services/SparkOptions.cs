namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Настройки интеграции с СПАРК (Интерфакс) через SOAP API (ifaborern.asmx).
/// Все значения — из конфигурационного файла (ADR-022).
/// </summary>
public class SparkOptions
{
    /// <summary>URL SOAP-сервиса СПАРК (например, http://sparkgatetest.interfax.ru/iFaxWebService/ifaborern.asmx). Задаётся в конфигурации.</summary>
    public string BaseUrl { get; init; } = "";

    /// <summary>Логин для аутентификации в SOAP-сервисе (Authmethod).</summary>
    public string Login { get; init; } = "";

    /// <summary>Пароль для аутентификации в SOAP-сервисе (Authmethod).</summary>
    public string Password { get; init; } = "";
}
