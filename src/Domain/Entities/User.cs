namespace SamorodinkaTech.Fiducia.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsExternal { get; set; }
    public bool PepAgreementSigned { get; set; }
    public DateTime? PepSignedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Онбординг внешних директоров
    public string? InvitationToken { get; set; }
    public DateTime? InvitationExpiresAt { get; set; }
    public bool DeclarationCompleted { get; set; }
    public string? DeclarationData { get; set; } // JSON с данными анкеты
    public bool PdnConsentGiven { get; set; } // Согласие на обработку ПДн
    public DateTime? PdnConsentAt { get; set; }
    public string? PdnConsentIp { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<CommitteeMember> CommitteeMembers { get; set; } = new List<CommitteeMember>();
    public ICollection<Bulletin> Bulletins { get; set; } = new List<Bulletin>();
}
