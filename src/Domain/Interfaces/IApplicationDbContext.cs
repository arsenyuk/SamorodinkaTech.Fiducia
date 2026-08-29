using Microsoft.EntityFrameworkCore;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

public interface IApplicationDbContext
{
    DbSet<PdnConsent> PdnConsents { get; }
    DbSet<PepAgreement> PepAgreements { get; }
    DbSet<IndependenceDeclaration> IndependenceDeclarations { get; }
    DbSet<User> Users { get; }
    DbSet<RefRole> Roles { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<Committee> Committees { get; }
    DbSet<CommitteeMember> CommitteeMembers { get; }
    DbSet<Meeting> Meetings { get; }
    DbSet<AgendaQuestion> AgendaQuestions { get; }
    DbSet<CommitteeTask> CommitteeTasks { get; }
    DbSet<Bulletin> Bulletins { get; }
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
    DbSet<LegalEntityBoardSettings> LegalEntityBoardSettings { get; }
    DbSet<LegalEntityCharter> LegalEntityCharters { get; }
    DbSet<LegalEntityVotingRules> LegalEntityVotingRules { get; }
    DbSet<LegalEntityEmailSettings> LegalEntityEmailSettings { get; }
    DbSet<LegalEntityExtraSettings> LegalEntityExtraSettings { get; }
    DbSet<FileEntry> Files { get; }
    DbSet<FileNotarization> FileNotarizations { get; }
    DbSet<ExtSparkCompany> ExtSparkCompanies { get; }
    DbSet<RefMeetingForm> MeetingForms { get; }
    DbSet<RefGdTerm> RefGdTerms { get; }
    DbSet<RefProtocolConfirmationMethod> RefProtocolConfirmationMethods { get; }
    DbSet<RefMeasurementUnit> RefMeasurementUnits { get; }
    DbSet<ExtSparkManager> ExtSparkManagers { get; }
    DbSet<ExtSparkFounder> ExtSparkFounders { get; }
    DbSet<ExtCbrFinOrgOrganization> ExtCbrFinOrgOrganizations { get; }
    DbSet<ExtCbrFinOrgLicense> ExtCbrFinOrgLicenses { get; }
    DbSet<Employee> Employees { get; }

    DbSet<ExternalAttractedPerson> ExternalAttractedPersons { get; }

    DbSet<EcosystemParticipant> EcosystemParticipants { get; }

    DbSet<TplOrgIntent> TplOrgIntents { get; }
    DbSet<TplOrgStage> TplOrgStages { get; }
    DbSet<TplOrgTaskOffer> TplOrgOffers { get; }
    DbSet<TplOrgMilestone> TplOrgMilestones { get; }
    DbSet<OrgIntent> OrgIntents { get; }
    DbSet<OrgStage> OrgStages { get; }
    DbSet<OrgTask> OrgTasks { get; }
    DbSet<OrgMilestone> OrgMilestones { get; }

    DbSet<TplOrgOfferRole> TplOrgOfferRoles { get; }

    DbSet<TrueConfTestMeeting> TrueConfTestMeetings { get; }
    DbSet<TrueConfTestQuestion> TrueConfTestQuestions { get; }
    DbSet<TrueConfTestAnswer> TrueConfTestAnswers { get; }

    DbSet<Contract> Contracts { get; }

    DbSet<SystemSetting> SystemSettings { get; }

    DbSet<BoardParticipant> BoardParticipants { get; }
    DbSet<BoardTreasuryShare> BoardTreasuryShares { get; }
    DbSet<BoardRegistryUpload> BoardRegistryUploads { get; }
    DbSet<BoardParticipantChange> BoardParticipantChanges { get; }

    DbSet<RefRequestType> RequestTypes { get; }

    DbSet<RefDocumentType> DocumentTypes { get; }
    DbSet<RefDocumentAccessMethod> DocumentAccessMethods { get; }
    DbSet<RefDocumentRefusalReason> DocumentRefusalReasons { get; }
    DbSet<RefDulType> RefDulTypes { get; }

    DbSet<ShareRequest> ShareRequests { get; }

    DbSet<ShareRequestSupport> ShareRequestSupports { get; }

    DbSet<ShareRequestFile> ShareRequestFiles { get; }

    DbSet<RefNotificationType> RefNotificationTypes { get; }

    DbSet<NotificationTemplate> NotificationTemplates { get; }

    System.Threading.Tasks.Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken = default);
}
