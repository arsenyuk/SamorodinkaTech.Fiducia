using SamorodinkaTech.Fiducia.Domain.Enums;

namespace SamorodinkaTech.Fiducia.Domain.Entities;

public class AgendaQuestion
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public Meeting Meeting { get; set; } = null!;
    public int SequenceNumber { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string ProposedResolution { get; set; } = string.Empty;
    public QuestionStatus Status { get; set; } = QuestionStatus.PENDING;

    public ICollection<Bulletin> Bulletins { get; set; } = new List<Bulletin>();
    public ICollection<CommitteeTask> CommitteeTasks { get; set; } = new List<CommitteeTask>();
}
