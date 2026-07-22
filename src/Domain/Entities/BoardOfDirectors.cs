namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Совет директоров (board_of_directors).
/// Головная запись для состава СД, утверждённого в рамках конкретного ОСА.
/// </summary>
public class BoardOfDirectors
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Ссылка на ОСА, в рамках которого утверждён данный состав СД (osa_meeting_id).</summary>
    public Guid OsaMeetingId { get; set; }

    /// <summary>Год проведения ГОСА (gosa_year). Денормализовано из OsaMeeting.GosaYear для отображения.</summary>
    public int? GosaYear { get; set; }

    /// <summary>Дата начала полномочий состава СД (started_at).</summary>
    public DateOnly? StartedAt { get; set; }

    /// <summary>Дата окончания полномочий состава СД (ended_at).</summary>
    public DateOnly? EndedAt { get; set; }

    /// <summary>Статус Совета директоров (status_id).</summary>
    public Guid StatusId { get; set; }

    public OsaMeeting? OsaMeeting { get; set; }
    public BoardOfDirectorsStatus? Status { get; set; }
}
