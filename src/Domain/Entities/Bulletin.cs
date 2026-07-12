using SamorodinkaTech.Fiducia.Domain.Enums;

namespace SamorodinkaTech.Fiducia.Domain.Entities;

public class Bulletin
{
    public int Id { get; set; }
    public Guid AgendaQuestionId { get; set; }
    public AgendaQuestion AgendaQuestion { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public VoteValue VoteValue { get; set; }
    public string? SpecialOpinion { get; set; }
    public SignatureType SignatureType { get; set; }
    public string SignatureValue { get; set; } = string.Empty;
    public DateTime SignedAt { get; set; }
    public bool IsCancelled { get; set; }
    public string? CancellationReason { get; set; }
}
