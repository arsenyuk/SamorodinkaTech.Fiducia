namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>Ответ (голос) на вопрос тестового заседания СД (trueconf_test_answer).</summary>
public class TrueConfTestAnswer
{
    /// <summary>Идентификатор ответа (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор вопроса (question_id).</summary>
    public Guid QuestionId { get; set; }

    /// <summary>Связь с вопросом.</summary>
    public TrueConfTestQuestion Question { get; set; } = null!;

    /// <summary>Имя проголосовавшего (user_name) — для теста из TrueConf.</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>Значение голоса (vote_value): ZA, PROTIV, VOZDERZHALSYA.</summary>
    public string VoteValue { get; set; } = string.Empty;

    /// <summary>Время голосования (voted_at).</summary>
    public DateTime VotedAt { get; set; } = DateTime.UtcNow;
}
