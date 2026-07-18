namespace SamorodinkaTech.Fiducia.Domain.Entities;

public class CommitteeMember
{
    /// <summary>Уникальный идентификатор (id).</summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>Идентификатор комитета (committee_id).</summary>
    public Guid CommitteeId { get; set; }
    /// <summary>Комитет.</summary>
    public Committee Committee { get; set; } = null!;

    /// <summary>Идентификатор пользователя (user_id).</summary>
    public Guid UserId { get; set; }
    /// <summary>Пользователь.</summary>
    public User User { get; set; } = null!;
}
