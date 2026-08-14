using SamorodinkaTech.Fiducia.Domain.Enums;

namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Заседание совета директоров (meetings).
/// </summary>
public class Meeting
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Номер заседания (meeting_number).</summary>
    public string? MeetingNumber { get; set; }

    /// <summary>Идентификатор формы проведения заседания (meeting_form_id).</summary>
    public Guid MeetingFormId { get; set; }

    /// <summary>Форма проведения заседания.</summary>
    public RefMeetingForm? MeetingForm { get; set; }

    /// <summary>Статус заседания (status): DRAFT, NOTIFIED, VOTING, PROTOCOL, ARCHIVE.</summary>
    public MeetingStatus Status { get; set; } = MeetingStatus.DRAFT;

    /// <summary>Дата и время начала голосования (voting_start_at).</summary>
    public DateTime? VotingStartAt { get; set; }

    /// <summary>Дата и время окончания голосования (voting_end_at).</summary>
    public DateTime? VotingEndAt { get; set; }

    /// <summary>Идентификатор создателя (created_by).</summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>Создатель заседания.</summary>
    public User? Creator { get; set; }

    /// <summary>Дата и время создания (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Вопросы повестки заседания.</summary>
    public ICollection<AgendaQuestion> AgendaQuestions { get; set; } = new List<AgendaQuestion>();
}
