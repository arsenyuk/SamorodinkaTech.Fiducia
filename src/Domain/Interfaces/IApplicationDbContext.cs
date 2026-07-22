using Microsoft.EntityFrameworkCore;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<Committee> Committees { get; }
    DbSet<CommitteeMember> CommitteeMembers { get; }
    DbSet<Meeting> Meetings { get; }
    DbSet<AgendaQuestion> AgendaQuestions { get; }
    DbSet<CommitteeTask> CommitteeTasks { get; }
    DbSet<Bulletin> Bulletins { get; }
    DbSet<SecurityAuditLog> SecurityAuditLogs { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<Okopf> Okopf { get; }
    DbSet<RefMonth> RefMonths { get; }
    DbSet<OsaForm> OsaForms { get; }
    DbSet<OsaMeeting> OsaMeetings { get; }
    DbSet<BoardMember> BoardMembers { get; }
    DbSet<BoardOfDirectors> BoardsOfDirectors { get; }
    DbSet<BoardOfDirectorsStatus> BoardOfDirectorsStatuses { get; }
    DbSet<BoardMemberType> BoardMemberTypes { get; }
    DbSet<BoardRole> BoardRoles { get; }
    DbSet<BoardMemberAppointment> BoardMemberAppointments { get; }
    DbSet<BoardMemberAppointmentStatus> BoardMemberAppointmentStatuses { get; }
    DbSet<ResignationReason> ResignationReasons { get; }
    DbSet<UserBoardMemberResignation> UserBoardMemberResignations { get; }
    DbSet<OsaMeetingFile> OsaMeetingFiles { get; }
    DbSet<LegalEntity> LegalEntities { get; }
    DbSet<CurrentWorkplace> CurrentWorkplaces { get; }
    DbSet<LegalEntityBoardSettings> LegalEntityBoardSettings { get; }
    DbSet<LegalEntityVotingRules> LegalEntityVotingRules { get; }
    DbSet<FileEntry> Files { get; }
    DbSet<ExtSparkCompany> ExtSparkCompanies { get; }
    DbSet<RefMeetingForm> MeetingForms { get; }
    DbSet<ExtSparkManager> ExtSparkManagers { get; }


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
