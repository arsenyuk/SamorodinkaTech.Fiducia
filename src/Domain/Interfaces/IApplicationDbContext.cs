using Microsoft.EntityFrameworkCore;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefRole> Roles { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<Committee> Committees { get; }
    DbSet<CommitteeMember> CommitteeMembers { get; }
    DbSet<Meeting> Meetings { get; }
    DbSet<AgendaQuestion> AgendaQuestions { get; }
    DbSet<CommitteeTask> CommitteeTasks { get; }
    DbSet<Bulletin> Bulletins { get; }
    DbSet<SecurityAuditLog> SecurityAuditLogs { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<RefOkopf> RefOkopf { get; }
    DbSet<RefMonth> RefMonths { get; }
    DbSet<RefOsaForm> OsaForms { get; }
    DbSet<OsaMeeting> OsaMeetings { get; }
    DbSet<BoardMember> BoardMembers { get; }
    DbSet<BoardOfDirectors> BoardsOfDirectors { get; }
    DbSet<RefBoardOfDirectorsStatus> BoardOfDirectorsStatuses { get; }
    DbSet<RefBoardMemberType> BoardMemberTypes { get; }
    DbSet<RefBoardRole> BoardRoles { get; }
    DbSet<BoardMemberAppointment> BoardMemberAppointments { get; }
    DbSet<RefBoardMemberAppointmentStatus> BoardMemberAppointmentStatuses { get; }
    DbSet<RefResignationReason> ResignationReasons { get; }
    DbSet<UserBoardMemberResignation> UserBoardMemberResignations { get; }
    DbSet<AgendaItem> AgendaItems { get; }
    DbSet<AgendaProposal> AgendaProposals { get; }
    DbSet<ElectionProposal> ElectionProposals { get; }
    DbSet<ElectionCandidacy> ElectionCandidacies { get; }
    DbSet<ElectionConsent> ElectionConsents { get; }
    DbSet<OsaMeetingFile> OsaMeetingFiles { get; }
    DbSet<LegalEntity> LegalEntities { get; }
    DbSet<CurrentWorkplace> CurrentWorkplaces { get; }
    DbSet<LegalEntityBoardSettings> LegalEntityBoardSettings { get; }
    DbSet<LegalEntityVotingRules> LegalEntityVotingRules { get; }
    DbSet<LegalEntityEmailSettings> LegalEntityEmailSettings { get; }
    DbSet<FileEntry> Files { get; }
    DbSet<ExtSparkCompany> ExtSparkCompanies { get; }
    DbSet<RefMeetingForm> MeetingForms { get; }
    DbSet<ExtSparkManager> ExtSparkManagers { get; }
    DbSet<ExtSparkFounder> ExtSparkFounders { get; }

    DbSet<TplOrgIntent> TplOrgIntents { get; }
    DbSet<TplOrgStage> TplOrgStages { get; }
    DbSet<TplOrgOffer> TplOrgOffers { get; }
    DbSet<TplOrgTask> TplOrgTasks { get; }
    DbSet<OrgIntent> OrgIntents { get; }
    DbSet<OrgStage> OrgStages { get; }
    DbSet<OrgOffer> OrgOffers { get; }
    DbSet<OrgTask> OrgTasks { get; }

    DbSet<TplOrgOfferRole> TplOrgOfferRoles { get; }
    System.Threading.Tasks.Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken = default);
}
