namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Согласие (или отказ) кандидата на участие в выборах на должность СД (election_consents).
/// Фиксирует волеизъявление кандидата, подписанное простой электронной подписью (ПЭП).
/// </summary>
public class ElectionConsent
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Ссылка на предложение кандидатур (proposal_id).</summary>
    public Guid ProposalId { get; set; }

    /// <summary>Ссылка на члена СД — кандидата (candidate_member_id).</summary>
    public Guid CandidateMemberId { get; set; }

    /// <summary>Согласие дано (consent_given): true — согласен, false — отказ.</summary>
    public bool ConsentGiven { get; set; }

    /// <summary>Токен для ссылки на страницу согласия (consent_token). Генерируется при отправке уведомления.</summary>
    public string ConsentToken { get; set; } = default!;

    /// <summary>Дата и время подписания / отказа (signed_at).</summary>
    public DateTime? SignedAt { get; set; }

    /// <summary>IP-адрес, с которого выполнено подписание (signed_ip).</summary>
    public string? SignedIp { get; set; }

    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ElectionProposal? Proposal { get; set; }
    public BoardMember? CandidateMember { get; set; }
}
