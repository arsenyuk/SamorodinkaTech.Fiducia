using SamorodinkaTech.Fiducia.Domain.Enums;

namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>Тестовое заседание Совета директоров (trueconf_test_meeting).</summary>
public class TrueConfTestMeeting
{
    /// <summary>Идентификатор заседания (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Название заседания (title).</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Описание заседания (description).</summary>
    public string? Description { get; set; }

    /// <summary>Идентификатор конференции TrueConf (trueconf_conference_id).</summary>
    public string? TrueConfConferenceId { get; set; }

    /// <summary>Ссылка для подключения к конференции (trueconf_join_link).</summary>
    public string? TrueConfJoinLink { get; set; }

    /// <summary>Состояние конференции в TrueConf (conference_state).</summary>
    public string? ConferenceState { get; set; }

    /// <summary>Фактическое начало заседания (started_at).</summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>Фактическое окончание заседания (ended_at).</summary>
    public DateTime? EndedAt { get; set; }

    /// <summary>Признак: все участники проголосовали (all_members_voted).</summary>
    public bool AllMembersVoted { get; set; }

    /// <summary>Признак принятия решения: true=принято, false=отклонено, null=не определено (decision_accepted).</summary>
    public bool? DecisionAccepted { get; set; }

    /// <summary>Статус заседания (status): PREPARING, IN_PROGRESS, COMPLETED.</summary>
    public string Status { get; set; } = "PREPARING";

    /// <summary>Дата создания (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Вопросы повестки.</summary>
    public ICollection<TrueConfTestQuestion> Questions { get; set; } = new List<TrueConfTestQuestion>();
}
