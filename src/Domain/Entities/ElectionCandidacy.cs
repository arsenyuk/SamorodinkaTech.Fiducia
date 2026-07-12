namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Кандидатура на выборы должностного лица СД (election_candidacies).
/// Фиксирует предложение и подтверждение каждой кандидатуры.
/// </summary>
public class ElectionCandidacy
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Ссылка на предложение (proposal_id).</summary>
    public Guid ProposalId { get; set; }

    /// <summary>Ссылка на члена СД — кандидата (candidate_member_id).</summary>
    public Guid CandidateMemberId { get; set; }

    /// <summary>Ссылка на члена СД, подтвердившего кандидатуру (confirmed_by_member_id).</summary>
    public Guid? ConfirmedByMemberId { get; set; }

    /// <summary>Дата подтверждения кандидатуры (confirmed_at).</summary>
    public DateTime? ConfirmedAt { get; set; }

    /// <summary>Дата и время создания (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ElectionProposal? Proposal { get; set; }
    public BoardMember? CandidateMember { get; set; }
    public BoardMember? ConfirmedByMember { get; set; }
}
