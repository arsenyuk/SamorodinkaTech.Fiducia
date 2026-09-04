namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>Настройки интеграции с ЕДИН (Mnemonios MPI). Адрес сервиса и идентификатор системы-источника.</summary>
public class EdinOptions
{
    /// <summary>Включена ли интеграция с ЕДИН.</summary>
    public bool Enabled { get; init; }

    /// <summary>URL API ЕДИН (например, http://localhost:5000).</summary>
    public string BaseUrl { get; init; } = "";

    /// <summary>Идентификатор системы-источника для ЕДИН.</summary>
    public string SourceSystemId { get; init; } = "fiducia";
}
