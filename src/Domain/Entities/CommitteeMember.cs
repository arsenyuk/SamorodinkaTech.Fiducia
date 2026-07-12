namespace SamorodinkaTech.Fiducia.Domain.Entities;

public class CommitteeMember
{
    public Guid CommitteeId { get; set; }
    public Committee Committee { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
