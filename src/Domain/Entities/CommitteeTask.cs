using SamorodinkaTech.Fiducia.Domain.Enums;

namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Задача комитета (committee_tasks).
/// </summary>
public class CommitteeTask
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Ссылка на комитет (committee_id).</summary>
    public Guid CommitteeId { get; set; }

    /// <summary>Навигация к комитету.</summary>
    public Committee Committee { get; set; } = null!;

    /// <summary>Ссылка на вопрос повестки (agenda_question_id).</summary>
    public Guid? AgendaQuestionId { get; set; }

    /// <summary>Навигация к вопросу повестки.</summary>
    public AgendaQuestion? AgendaQuestion { get; set; }

    /// <summary>Описание задачи (task_description).</summary>
    public string TaskDescription { get; set; } = string.Empty;

    /// <summary>Крайний срок (deadline_at).</summary>
    public DateTime DeadlineAt { get; set; }

    /// <summary>Статус задачи (status).</summary>
    public CommitteeTaskStatus Status { get; set; } = CommitteeTaskStatus.IN_WORK;

    /// <summary>Создатель задачи (created_by).</summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>Навигация к создателю.</summary>
    public User? Creator { get; set; }

    /// <summary>Дата создания (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
