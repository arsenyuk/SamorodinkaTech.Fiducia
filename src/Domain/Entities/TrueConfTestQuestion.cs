namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>Вопрос повестки тестового заседания СД (trueconf_test_question).</summary>
public class TrueConfTestQuestion
{
    /// <summary>Идентификатор вопроса (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор заседания (meeting_id).</summary>
    public Guid MeetingId { get; set; }

    /// <summary>Связь с заседанием.</summary>
    public TrueConfTestMeeting Meeting { get; set; } = null!;

    /// <summary>Порядковый номер вопроса (sequence_number).</summary>
    public int SequenceNumber { get; set; }

    /// <summary>Текст вопроса (question_text).</summary>
    public string QuestionText { get; set; } = string.Empty;

    /// <summary>Проект решения (proposed_resolution).</summary>
    public string ProposedResolution { get; set; } = string.Empty;

    /// <summary>Идентификатор опроса в TrueConf (trueconf_poll_id).</summary>
    public string? TrueConfPollId { get; set; }

    /// <summary>Состояние опроса в TrueConf: active, closed (poll_state).</summary>
    public string? PollState { get; set; }

    /// <summary>Статус вопроса (status): PENDING, VOTED, ACCEPTED, REJECTED.</summary>
    public string Status { get; set; } = "PENDING";

    /// <summary>Дата создания (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Ответы (голоса) на вопрос.</summary>
    public ICollection<TrueConfTestAnswer> Answers { get; set; } = new List<TrueConfTestAnswer>();
}
