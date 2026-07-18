namespace SamorodinkaTech.Fiducia.Domain.Models.TrueConf;

/// <summary>
/// Конференция TrueConf Server.
/// </summary>
public class TrueConfConference
{
    /// <summary>Идентификатор конференции (id).</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Название конференции (display_name).</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Состояние конференции (state).</summary>
    public string State { get; init; } = string.Empty;

    /// <summary>Расписание конференции (schedule).</summary>
    public TrueConfSchedule? Schedule { get; init; }

    /// <summary>Ссылка для подключения (join_link).</summary>
    public string? JoinLink { get; init; }
}

/// <summary>
/// Расписание конференции TrueConf.
/// </summary>
public class TrueConfSchedule
{
    /// <summary>Время начала в Unix timestamp (start_time).</summary>
    public long StartTime { get; init; }

    /// <summary>Длительность в секундах (duration).</summary>
    public long Duration { get; init; }

    /// <summary>Тип расписания: 1 — запланированная (type).</summary>
    public int Type { get; init; }
}
