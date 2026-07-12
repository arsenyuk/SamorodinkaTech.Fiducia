using SamorodinkaTech.Fiducia.Domain.Enums;

namespace SamorodinkaTech.Fiducia.Domain.Entities;

public class CommitteeTask
{
    public int Id { get; set; }
    public Guid CommitteeId { get; set; }
    public Committee Committee { get; set; } = null!;
    public Guid? AgendaQuestionId { get; set; }
    public AgendaQuestion? AgendaQuestion { get; set; }
    public string TaskDescription { get; set; } = string.Empty;
    public DateTime DeadlineAt { get; set; }
    public CommitteeTaskStatus Status { get; set; } = CommitteeTaskStatus.IN_WORK;
    public Guid? CreatedBy { get; set; }
    public User? Creator { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
