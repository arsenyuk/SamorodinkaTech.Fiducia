namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Настройки интеграции с API «Отправка» Почты России (ADR-022).
/// Все значения — из конфигурационного файла.
/// </summary>
public class PochtaRussiaOptions
{
    /// <summary>URL API (по умолчанию https://otpravka-api.pochta.ru).</summary>
    public string BaseUrl { get; init; } = "https://otpravka-api.pochta.ru";

    /// <summary>Флаг включения интеграции. false — клиент не регистрируется в DI.</summary>
    public bool Enabled { get; init; }

    /// <summary>Токен авторизации приложения.</summary>
    public string AccessToken { get; init; } = "";

    /// <summary>Логин пользователя для X-User-Authorization.</summary>
    public string Login { get; init; } = "";

    /// <summary>Пароль пользователя для X-User-Authorization.</summary>
    public string Password { get; init; } = "";
}
