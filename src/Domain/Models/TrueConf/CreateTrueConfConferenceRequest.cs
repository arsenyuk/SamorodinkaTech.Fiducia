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

    /// <summary>Тема конференции (topic).</summary>
    public string Topic { get; init; } = string.Empty;

    /// <summary>Идентификатор организатора (owner) — user_id в TrueConf.</summary>
    public string Owner { get; init; } = string.Empty;

    /// <summary>Приглашённые участники (invitations).</summary>
    public List<TrueConfInvitation> Invitations { get; init; } = new();
}

/// <summary>
/// Приглашение участника в конференцию.
/// </summary>
public class TrueConfInvitation
{
    /// <summary>Идентификатор пользователя в TrueConf (id).</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Роль: participant, moderator.</summary>
    public string Role { get; init; } = "participant";
}
