namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Настройки SMTP-сервера для отправки email (ADR-022).
/// Все значения — из конфигурационного файла.
/// </summary>
public class SmtpOptions
{
    /// <summary>Адрес SMTP-сервера.</summary>
    public string Host { get; set; } = "";

    /// <summary>Порт SMTP-сервера (по умолчанию 587).</summary>
    public int Port { get; set; } = 587;

    /// <summary>Имя пользователя SMTP.</summary>
    public string User { get; set; } = "";

    /// <summary>Пароль SMTP.</summary>
    public string Password { get; set; } = "";

    /// <summary>Адрес отправителя (From).</summary>
    public string From { get; set; } = "";

    /// <summary>Использовать SSL (по умолчанию true).</summary>
    public bool UseSsl { get; set; } = true;

    /// <summary>Флаг включения интеграции.</summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Email для перехвата в dev-режиме. Если задан, все письма
    /// отправляются на этот адрес вместо реального получателя.
    /// Хранится в .env, не в git.
    /// </summary>
    public string DevOverrideTo { get; set; } = "";
}
