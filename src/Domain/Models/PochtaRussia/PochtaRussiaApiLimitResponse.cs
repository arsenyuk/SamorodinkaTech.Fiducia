namespace SamorodinkaTech.Fiducia.Domain.Models.PochtaRussia;

/// <summary>
/// Ответ на запрос текущего количества запросов по API (GET /1.0/settings/limit).
/// </summary>
public class PochtaRussiaApiLimitResponse
{
    /// <summary>Количество запросов по API, разрешённых для клиента в сутки.</summary>
    public int AllowedCount { get; init; }

    /// <summary>Текущее количество использованных запросов по API.</summary>
    public int CurrentCount { get; init; }
}
