namespace SamorodinkaTech.Fiducia.Domain.Models.TrueConf;

/// <summary>
/// Запрос на создание конференции TrueConf для заседания совета директоров.
/// </summary>
public class CreateTrueConfConferenceRequest
{
    /// <summary>Название конференции (display_name).</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Время начала (Unix timestamp).</summary>
    public long StartTime { get; init; }

    /// <summary>Длительность в секундах.</summary>
    public long Duration { get; init; }

    /// <summary>Тег для фильтрации (tag).</summary>
    public string? Tag { get; init; }
}
