namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Настройки интеграции с SOAP-сервисом ЦБ РФ (FinOrg.asmx) (ADR-022).
/// Все значения — из конфигурационного файла.
/// </summary>
public class CbrFinOrgOptions
{
    /// <summary>URL SOAP-сервиса FinOrg (по умолчанию https://cbr.ru/FO_ZoomWS/FinOrg.asmx).</summary>
    public string BaseUrl { get; init; } = "https://cbr.ru/FO_ZoomWS/FinOrg.asmx";

    /// <summary>Флаг включения интеграции. false — клиент не регистрируется в DI.</summary>
    public bool Enabled { get; init; }
}
