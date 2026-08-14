using SamorodinkaTech.Fiducia.Domain.Enums;

namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Бюллетень голосования по вопросу повестки (bulletins).
/// </summary>
public class Bulletin
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Ссылка на вопрос повестки (agenda_question_id).</summary>
    public Guid AgendaQuestionId { get; set; }

    /// <summary>Вопрос повестки.</summary>
    public AgendaQuestion AgendaQuestion { get; set; } = null!;

    /// <summary>Ссылка на пользователя (user_id).</summary>
    public Guid UserId { get; set; }

    /// <summary>Пользователь.</summary>
    public User User { get; set; } = null!;

    /// <summary>Значение голоса (vote_value): ZA, PROTIV, VOZDERZHALSYA, CONFLICT.</summary>
    public VoteValue VoteValue { get; set; }

    /// <summary>Особое мнение (special_opinion).</summary>
    public string? SpecialOpinion { get; set; }

    /// <summary>Тип подписи (signature_type): PEP или UKEP.</summary>
    public SignatureType SignatureType { get; set; }

    /// <summary>Значение подписи (signature_value).</summary>
    public string SignatureValue { get; set; } = string.Empty;

    /// <summary>Дата и время подписания (signed_at).</summary>
    public DateTime SignedAt { get; set; }

    /// <summary>Признак отмены бюллетеня (is_cancelled).</summary>
    public bool IsCancelled { get; set; }

    /// <summary>Причина отмены (cancellation_reason).</summary>
    public string? CancellationReason { get; set; }
}
