using SamorodinkaTech.Fiducia.Domain.Enums;

namespace SamorodinkaTech.Fiducia.Domain.Entities;

public class Meeting
{
    public Guid Id { get; set; }
    public string? MeetingNumber { get; set; }
    public MeetingForm MeetingForm { get; set; }
    public MeetingStatus Status { get; set; } = MeetingStatus.DRAFT;
    public DateTime? VotingStartAt { get; set; }
    public DateTime? VotingEndAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public User? Creator { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<AgendaQuestion> AgendaQuestions { get; set; } = new List<AgendaQuestion>();
}
