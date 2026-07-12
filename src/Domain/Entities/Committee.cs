using SamorodinkaTech.Fiducia.Domain.Enums;

namespace SamorodinkaTech.Fiducia.Domain.Entities;

public class Committee
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public BehaviorType BehaviorType { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? ChairId { get; set; }
    public User? Chair { get; set; }
    public Guid? SecretaryId { get; set; }
    public User? Secretary { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CommitteeMember> Members { get; set; } = new List<CommitteeMember>();
    public ICollection<CommitteeTask> Tasks { get; set; } = new List<CommitteeTask>();
}
