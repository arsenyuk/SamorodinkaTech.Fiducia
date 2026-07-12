namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Предложение по добавлению вопроса в повестку Совета директоров (agenda_proposals).
/// Подаётся через публичную форму без авторизации.
/// </summary>
public class AgendaProposal
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>ФИО заявителя (submitter_name).</summary>
    public string SubmitterName { get; set; } = default!;

    /// <summary>Email заявителя (submitter_email).</summary>
    public string? SubmitterEmail { get; set; }

    /// <summary>Суть предлагаемого вопроса (proposal_text).</summary>
    public string ProposalText { get; set; } = default!;

    /// <summary>Статус (status): SUBMITTED, REVIEWED, ACCEPTED, REJECTED.</summary>
    public string Status { get; set; } = "SUBMITTED";

    /// <summary>Дата и время создания (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
