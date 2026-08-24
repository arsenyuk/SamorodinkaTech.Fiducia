using Microsoft.EntityFrameworkCore;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Enums;
using SamorodinkaTech.Fiducia.Domain.Interfaces;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence;

public class FiduciaDbContext : Microsoft.EntityFrameworkCore.DbContext, IApplicationDbContext
{
    public FiduciaDbContext(DbContextOptions<FiduciaDbContext> options) : base(options) { }

    public DbSet<Person> Persons => Set<Person>();
    public DbSet<PdnConsent> PdnConsents => Set<PdnConsent>();
    public DbSet<PepAgreement> PepAgreements => Set<PepAgreement>();
    public DbSet<IndependenceDeclaration> IndependenceDeclarations => Set<IndependenceDeclaration>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefRole> Roles => Set<RefRole>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Committee> Committees => Set<Committee>();
    public DbSet<CommitteeMember> CommitteeMembers => Set<CommitteeMember>();
    public DbSet<Meeting> Meetings => Set<Meeting>();
    public DbSet<AgendaQuestion> AgendaQuestions => Set<AgendaQuestion>();
    public DbSet<CommitteeTask> CommitteeTasks => Set<CommitteeTask>();
    public DbSet<Bulletin> Bulletins => Set<Bulletin>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<RefNotificationType> RefNotificationTypes => Set<RefNotificationType>();
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
    public DbSet<RefOkopf> RefOkopf => Set<RefOkopf>();
    public DbSet<RefMonth> RefMonths => Set<RefMonth>();
    public DbSet<RefOsaForm> OsaForms => Set<RefOsaForm>();
    public DbSet<OsaMeeting> OsaMeetings => Set<OsaMeeting>();
    public DbSet<BoardMember> BoardMembers => Set<BoardMember>();
    public DbSet<BoardOfDirectors> BoardsOfDirectors => Set<BoardOfDirectors>();
    public DbSet<RefBoardOfDirectorsStatus> BoardOfDirectorsStatuses => Set<RefBoardOfDirectorsStatus>();
    public DbSet<RefBoardMemberType> BoardMemberTypes => Set<RefBoardMemberType>();
    public DbSet<RefBoardRole> BoardRoles => Set<RefBoardRole>();
    public DbSet<BoardMemberAppointment> BoardMemberAppointments => Set<BoardMemberAppointment>();
    public DbSet<RefBoardMemberAppointmentStatus> BoardMemberAppointmentStatuses => Set<RefBoardMemberAppointmentStatus>();
    public DbSet<RefResignationReason> ResignationReasons => Set<RefResignationReason>();
    public DbSet<UserBoardMemberResignation> UserBoardMemberResignations => Set<UserBoardMemberResignation>();
    public DbSet<OsaMeetingFile> OsaMeetingFiles => Set<OsaMeetingFile>();
    public DbSet<LegalEntity> LegalEntities => Set<LegalEntity>();
    public DbSet<CurrentWorkplace> CurrentWorkplaces => Set<CurrentWorkplace>();
    public DbSet<LegalEntityBoardSettings> LegalEntityBoardSettings => Set<LegalEntityBoardSettings>();
    public DbSet<LegalEntityVotingRules> LegalEntityVotingRules => Set<LegalEntityVotingRules>();
    public DbSet<LegalEntityEmailSettings> LegalEntityEmailSettings => Set<LegalEntityEmailSettings>();
    public DbSet<LegalEntityExtraSettings> LegalEntityExtraSettings => Set<LegalEntityExtraSettings>();
    public DbSet<LegalEntityDocumentAccess> LegalEntityDocumentAccesses => Set<LegalEntityDocumentAccess>();
    public DbSet<RefStandardCharter> RefStandardCharters => Set<RefStandardCharter>();
    public DbSet<LegalEntityCharter> LegalEntityCharters => Set<LegalEntityCharter>();
    public DbSet<AgendaItem> AgendaItems => Set<AgendaItem>();
    public DbSet<AgendaProposal> AgendaProposals => Set<AgendaProposal>();
    public DbSet<ElectionProposal> ElectionProposals => Set<ElectionProposal>();
    public DbSet<ElectionCandidacy> ElectionCandidacies => Set<ElectionCandidacy>();
    public DbSet<ElectionConsent> ElectionConsents => Set<ElectionConsent>();
    public DbSet<FileEntry> Files => Set<FileEntry>();
    public DbSet<ExtSparkCompany> ExtSparkCompanies => Set<ExtSparkCompany>();
    public DbSet<RefMeetingForm> MeetingForms => Set<RefMeetingForm>();
    public DbSet<RefGdTerm> RefGdTerms => Set<RefGdTerm>();
    public DbSet<RefProtocolConfirmationMethod> RefProtocolConfirmationMethods => Set<RefProtocolConfirmationMethod>();
    public DbSet<RefMeasurementUnit> RefMeasurementUnits => Set<RefMeasurementUnit>();
    public DbSet<ExtSparkManager> ExtSparkManagers => Set<ExtSparkManager>();
    public DbSet<ExtSparkFounder> ExtSparkFounders => Set<ExtSparkFounder>();
    public DbSet<ExtCbrFinOrgOrganization> ExtCbrFinOrgOrganizations => Set<ExtCbrFinOrgOrganization>();
    public DbSet<ExtCbrFinOrgLicense> ExtCbrFinOrgLicenses => Set<ExtCbrFinOrgLicense>();
    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<ExternalAttractedPerson> ExternalAttractedPersons => Set<ExternalAttractedPerson>();

    public DbSet<TplOrgIntent> TplOrgIntents => Set<TplOrgIntent>();
    public DbSet<TplOrgStage> TplOrgStages => Set<TplOrgStage>();
    public DbSet<TplOrgTaskOffer> TplOrgOffers => Set<TplOrgTaskOffer>();
    public DbSet<TplOrgMilestone> TplOrgMilestones => Set<TplOrgMilestone>();
    public DbSet<OrgIntent> OrgIntents => Set<OrgIntent>();
    public DbSet<OrgStage> OrgStages => Set<OrgStage>();
    public DbSet<OrgTask> OrgTasks => Set<OrgTask>();
    public DbSet<OrgMilestone> OrgMilestones => Set<OrgMilestone>();

    public DbSet<TplOrgOfferRole> TplOrgOfferRoles => Set<TplOrgOfferRole>();

    public DbSet<TrueConfTestMeeting> TrueConfTestMeetings => Set<TrueConfTestMeeting>();
    public DbSet<TrueConfTestQuestion> TrueConfTestQuestions => Set<TrueConfTestQuestion>();
    public DbSet<TrueConfTestAnswer> TrueConfTestAnswers => Set<TrueConfTestAnswer>();

    public DbSet<AoContractor> AoContractors => Set<AoContractor>();
    public DbSet<LlcManagementContract> LlcManagementContracts => Set<LlcManagementContract>();

    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    public DbSet<BoardParticipant> BoardParticipants => Set<BoardParticipant>();
    public DbSet<BoardTreasuryShare> BoardTreasuryShares => Set<BoardTreasuryShare>();
    public DbSet<BoardRegistryUpload> BoardRegistryUploads => Set<BoardRegistryUpload>();
    public DbSet<BoardParticipantChange> BoardParticipantChanges => Set<BoardParticipantChange>();
    public DbSet<ShareRequest> ShareRequests => Set<ShareRequest>();
    public DbSet<RefRequestType> RequestTypes => Set<RefRequestType>();
    public DbSet<RefDocumentType> DocumentTypes => Set<RefDocumentType>();
    public DbSet<RefDocumentAccessMethod> DocumentAccessMethods => Set<RefDocumentAccessMethod>();
    public DbSet<RefDocumentRefusalReason> DocumentRefusalReasons => Set<RefDocumentRefusalReason>();
    public DbSet<Notarization> Notarizations => Set<Notarization>();
    public DbSet<ShareRequestSupport> ShareRequestSupports => Set<ShareRequestSupport>();
    public DbSet<ShareRequestFile> ShareRequestFiles => Set<ShareRequestFile>();
    public DbSet<ShareRequestItem> ShareRequestItems => Set<ShareRequestItem>();
    public DbSet<ShareRequestItemFile> ShareRequestItemFiles => Set<ShareRequestItemFile>();

    // Junction-таблицы файлов (BDR-011)
    public DbSet<MeetingFile> MeetingFiles => Set<MeetingFile>();
    public DbSet<AgendaQuestionFile> AgendaQuestionFiles => Set<AgendaQuestionFile>();
    public DbSet<CommitteeTaskFile> CommitteeTaskFiles => Set<CommitteeTaskFile>();
    public DbSet<OrgTaskFile> OrgTaskFiles => Set<OrgTaskFile>();
    public DbSet<CommitteeFile> CommitteeFiles => Set<CommitteeFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FiduciaDbContext).Assembly);

        // Конфигурация LegalEntity → ref_okopf по UUID
        modelBuilder.Entity<LegalEntity>(b =>
        {
            b.ToTable("legal_entities");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Name).HasColumnName("name").IsRequired();
            b.Property(x => x.ShortName).HasColumnName("short_name");
            b.Property(x => x.Inn).HasColumnName("inn").HasMaxLength(12);
            b.Property(x => x.Ogrn).HasColumnName("ogrn").HasMaxLength(15);
            b.Property(x => x.OkopfId).HasColumnName("okopf_id");
            b.Property(x => x.StandardCharterId).HasColumnName("standard_charter_id");
            b.HasIndex(x => x.Name).HasDatabaseName("ix_legal_entities_name");
            b.HasIndex(x => x.Inn).HasDatabaseName("ix_legal_entities_inn");
            b.HasIndex(x => x.Ogrn).HasDatabaseName("ix_legal_entities_ogrn");
            b.HasOne(x => x.RefOkopf)
             .WithMany()
             .HasForeignKey(x => x.OkopfId)
             .HasPrincipalKey(o => o.Id);
            b.HasOne(x => x.StandardCharter)
             .WithMany()
             .HasForeignKey(x => x.StandardCharterId)
             .HasPrincipalKey(c => c.Id);
        });

        modelBuilder.Entity<RefStandardCharter>(b =>
        {
            b.ToTable("ref_standard_charter");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Number).HasColumnName("number").HasMaxLength(2).IsRequired();
            b.HasIndex(x => x.Number).IsUnique().HasDatabaseName("ux_ref_standard_charter_number");
            b.Property(x => x.ExitAllowed).HasColumnName("exit_allowed");
            b.Property(x => x.TransferToParticipantsWithoutConsent).HasColumnName("transfer_to_participants_without_consent");
            b.Property(x => x.TransferToThirdPartiesWithoutConsent).HasColumnName("transfer_to_third_parties_without_consent");
            b.Property(x => x.PreemptiveRight).HasColumnName("preemptive_right");
            b.Property(x => x.InheritanceWithoutConsent).HasColumnName("inheritance_without_consent");
            b.Property(x => x.ExecutiveBody).HasColumnName("executive_body").HasMaxLength(1);
            b.Property(x => x.ProtocolConfirmationMethodId).HasColumnName("protocol_confirmation_method_id");
            b.HasOne(x => x.ProtocolConfirmationMethod).WithMany().HasForeignKey(x => x.ProtocolConfirmationMethodId);
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.Property(x => x.CreatedBy).HasColumnName("created_by").IsRequired();
        });

        modelBuilder.Entity<LegalEntityCharter>(b =>
        {
            b.ToTable("legal_entity_charter");
            b.HasKey(x => x.LegalEntityId);
            b.Property(x => x.LegalEntityId).HasColumnName("legal_entity_id");
            b.Property(x => x.ExitAllowed).HasColumnName("exit_allowed");
            b.Property(x => x.ExitAllowedMinSharePercent).HasColumnName("exit_allowed_min_share_percent");
            b.Property(x => x.ExitAllowedMaxSharePercent).HasColumnName("exit_allowed_max_share_percent");
            b.Property(x => x.ExitConditionDescription).HasColumnName("exit_condition_description");
            b.Property(x => x.ExitRequiresUnanimousOsu).HasColumnName("exit_requires_unanimous_osu");
            b.Property(x => x.TransferToParticipantsWithoutConsent).HasColumnName("transfer_to_participants_without_consent");
            b.Property(x => x.TransferToThirdParties).HasColumnName("transfer_to_third_parties").HasMaxLength(20).IsRequired();
            b.Property(x => x.PreemptiveRight).HasColumnName("preemptive_right");
            b.Property(x => x.InheritanceWithoutConsent).HasColumnName("inheritance_without_consent");
            b.Property(x => x.ExecutiveBody).HasColumnName("executive_body").HasMaxLength(1);
            b.Property(x => x.ProtocolConfirmationMethodId).HasColumnName("protocol_confirmation_method_id");
            b.HasOne(x => x.ProtocolConfirmationMethod).WithMany().HasForeignKey(x => x.ProtocolConfirmationMethodId);
            b.Property(x => x.CharterDocumentId).HasColumnName("charter_document_id");
            b.Property(x => x.BoardRegulationDocumentId).HasColumnName("board_regulation_document_id");
            b.Property(x => x.CommitteeRegulationDocumentId).HasColumnName("committee_regulation_document_id");
            b.Property(x => x.MandatoryAudit).HasColumnName("mandatory_audit");
            b.Property(x => x.HasRevisionCommission).HasColumnName("has_revision_commission");
            b.Property(x => x.HasBoardOfDirectors).HasColumnName("has_board_of_directors");
            b.Property(x => x.GdTermId).HasColumnName("gd_term_id");
            b.Property(x => x.VosuThresholdPercent).HasColumnName("vosu_threshold_percent");
            b.Property(x => x.BoardDecidesConveningOsu).HasColumnName("board_decides_convening_osu");
            b.HasOne(x => x.GdTerm).WithMany().HasForeignKey(x => x.GdTermId);
            b.HasOne(x => x.CharterDocument).WithMany().HasForeignKey(x => x.CharterDocumentId);
            b.HasOne(x => x.BoardRegulationDocument).WithMany().HasForeignKey(x => x.BoardRegulationDocumentId);
            b.HasOne(x => x.CommitteeRegulationDocument).WithMany().HasForeignKey(x => x.CommitteeRegulationDocumentId);
            b.HasOne(x => x.LegalEntity).WithOne().HasForeignKey<LegalEntityCharter>(x => x.LegalEntityId);
        });

        modelBuilder.Entity<RefMonth>(b =>
        {
            b.ToTable("ref_month");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Code).HasColumnName("code").HasMaxLength(2).IsRequired();
            b.Property(x => x.Name).HasColumnName("name").HasMaxLength(20).IsRequired();
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.Property(x => x.CreatedBy).HasColumnName("created_by").IsRequired();
            b.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ux_ref_month_code");
        });

        modelBuilder.Entity<RefGdTerm>(b =>
        {
            b.ToTable("ref_gd_term");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Code).HasColumnName("code").HasMaxLength(20).IsRequired();
            b.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ux_ref_gd_term_code");
            b.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            b.Property(x => x.DurationYears).HasColumnName("duration_years");
            b.Property(x => x.SortOrder).HasColumnName("sort_order");
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.Property(x => x.CreatedBy).HasColumnName("created_by").IsRequired();
        });

        modelBuilder.Entity<RefProtocolConfirmationMethod>(b =>
        {
            b.ToTable("ref_protocol_confirmation_method");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Code).HasColumnName("code").HasMaxLength(20).IsRequired();
            b.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ux_ref_protocol_confirmation_method_code");
            b.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            b.Property(x => x.SortOrder).HasColumnName("sort_order");
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.Property(x => x.CreatedBy).HasColumnName("created_by").IsRequired();
        });

        modelBuilder.Entity<RefMeasurementUnit>(b =>
        {
            b.ToTable("ref_measurement_unit");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Code).HasColumnName("code").HasMaxLength(20).IsRequired();
            b.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ux_ref_measurement_unit_code");
            b.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            b.Property(x => x.ShortName).HasColumnName("short_name").HasMaxLength(50).IsRequired();
            b.Property(x => x.SortOrder).HasColumnName("sort_order");
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.Property(x => x.CreatedBy).HasColumnName("created_by").IsRequired();
        });

        modelBuilder.Entity<CurrentWorkplace>(b =>
        {
            b.ToTable("current_workplace");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.FullName).HasColumnName("full_name").IsRequired();
            b.Property(x => x.Position).HasColumnName("position").HasMaxLength(200);
            b.Property(x => x.LastSelectedLegalEntityId).HasColumnName("last_selected_legal_entity_id");
        });

        modelBuilder.Entity<LegalEntityBoardSettings>(b =>
        {
            b.ToTable("legal_entity_board_settings");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            // Интервал ГОСА (date без времени)
            b.Property(x => x.GosaWindowStart)
             .HasColumnName("gosa_window_start")
             .HasColumnType("date");
            b.Property(x => x.GosaWindowEnd)
              .HasColumnName("gosa_window_end")
              .HasColumnType("date");
            b.Property(x => x.DeputyChairProvided).HasColumnName("deputy_chair_provided").HasDefaultValue(false);
            b.Property(x => x.SecretaryProvided).HasColumnName("secretary_provided").HasDefaultValue(true);
            b.Property(x => x.SecretarySignsProtocols).HasColumnName("secretary_signs_protocols").HasDefaultValue(false);
            b.Property(x => x.CommitteesMandatory).HasColumnName("committees_mandatory").HasDefaultValue(false);
            b.Property(x => x.CommitteesDefinedByDocuments).HasColumnName("committees_defined_by_documents").HasDefaultValue(false);
            b.Property(x => x.MaxCommitteesPerMemberDefined).HasColumnName("max_committees_per_member_defined").HasDefaultValue(false);
            b.Property(x => x.MaxCommitteesPerMember).HasColumnName("max_committees_per_member");
            b.Property(x => x.MaxCommitteesHeadedPerMemberDefined).HasColumnName("max_committees_headed_per_member_defined").HasDefaultValue(false);
            b.Property(x => x.MaxCommitteesHeadedPerMember).HasColumnName("max_committees_headed_per_member");
            b.Property(x => x.MinCommitteeMembersDefined).HasColumnName("min_committee_members_defined").HasDefaultValue(false);
            b.Property(x => x.MinCommitteeMembers).HasColumnName("min_committee_members");
            b.Property(x => x.CommitteeQuorumDefined).HasColumnName("committee_quorum_defined").HasDefaultValue(false);
            b.Property(x => x.CommitteeQuorumPercent).HasColumnName("committee_quorum_percent");
            b.Property(x => x.JointCommitteeQuorumDefined).HasColumnName("joint_committee_quorum_defined").HasDefaultValue(false);
            b.Property(x => x.JointCommitteeQuorumPercent).HasColumnName("joint_committee_quorum_percent");
        });

        modelBuilder.Entity<LegalEntityVotingRules>(b =>
        {
            b.ToTable("legal_entity_voting_rules");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.LegalEntityId).HasColumnName("legal_entity_id").IsRequired();
            b.Property(x => x.QuorumPercent).HasColumnName("quorum_percent").HasDefaultValue(50);
            b.Property(x => x.ChairTiebreaker).HasColumnName("chair_tiebreaker").HasDefaultValue(false);
            b.Property(x => x.AbsenteeOpinions).HasColumnName("absentee_opinions").HasDefaultValue(false);
            b.Property(x => x.QualifiedMajorityPercent).HasColumnName("qualified_majority_percent").HasDefaultValue(75);
            b.Property(x => x.InPersonAllowed).HasColumnName("in_person_allowed").HasDefaultValue(true);
            b.Property(x => x.AbsenteeAllowed).HasColumnName("absentee_allowed").HasDefaultValue(false);
            b.Property(x => x.MixedAllowed).HasColumnName("mixed_allowed").HasDefaultValue(false);
            b.Property(x => x.DocumentFlow).HasColumnName("document_flow").HasDefaultValue(DocumentFlowType.Paper);
            b.Property(x => x.SpotByElection).HasColumnName("spot_by_election").HasDefaultValue(false);
            b.Property(x => x.FirstMeetingDeadlineDays).HasColumnName("first_meeting_deadline_days").HasDefaultValue(30);
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.HasOne(x => x.LegalEntity)
             .WithMany()
             .HasForeignKey(x => x.LegalEntityId);
        });

        modelBuilder.Entity<LegalEntityEmailSettings>(b =>
        {
            b.ToTable("legal_entity_email_settings");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.LegalEntityId).HasColumnName("legal_entity_id").IsRequired();
            b.Property(x => x.HeaderEnabled).HasColumnName("header_enabled").HasDefaultValue(false);
            b.Property(x => x.HeaderMarkdown).HasColumnName("header_markdown").HasColumnType("text");
            b.Property(x => x.FooterEnabled).HasColumnName("footer_enabled").HasDefaultValue(false);
            b.Property(x => x.FooterMarkdown).HasColumnName("footer_markdown").HasColumnType("text");
            b.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.HasOne(x => x.LegalEntity)
             .WithMany()
             .HasForeignKey(x => x.LegalEntityId);
        });

        modelBuilder.Entity<LegalEntityExtraSettings>(b =>
        {
            b.ToTable("legal_entity_extra_settings");
            b.HasKey(x => x.LegalEntityId);
            b.Property(x => x.LegalEntityId).HasColumnName("legal_entity_id");
            b.Property(x => x.NotaryListApproved).HasColumnName("notary_list_approved").HasDefaultValue(false);
            b.Property(x => x.NotaryListOsaMeetingId).HasColumnName("notary_list_osa_meeting_id");
            b.Property(x => x.NotaryListDecisionDate).HasColumnName("notary_list_decision_date");
            b.HasOne(x => x.LegalEntity)
             .WithOne()
             .HasForeignKey<LegalEntityExtraSettings>(x => x.LegalEntityId);
            b.HasOne(x => x.NotaryListOsaMeeting)
             .WithMany()
             .HasForeignKey(x => x.NotaryListOsaMeetingId);
        });

        modelBuilder.Entity<RefMeetingForm>(b =>
        {
            b.ToTable("ref_meeting_form");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Code).HasColumnName("code").HasMaxLength(10).IsRequired();
            b.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            b.Property(x => x.ShortName).HasColumnName("short_name").HasMaxLength(50);
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.Property(x => x.CreatedBy).HasColumnName("created_by").IsRequired();
            b.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ux_ref_meeting_form_code");
        });

        modelBuilder.Entity<RefOsaForm>(b =>
        {
            b.ToTable("ref_osa_form");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Code).HasColumnName("code").HasMaxLength(10).IsRequired();
            b.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            b.Property(x => x.ShortName).HasColumnName("short_name").HasMaxLength(50);
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.Property(x => x.CreatedBy).HasColumnName("created_by").IsRequired();
            b.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ux_ref_osa_form_code");
        });

        modelBuilder.Entity<OsaMeeting>(b =>
        {
            b.ToTable("osa_meetings");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.LegalEntityId).HasColumnName("legal_entity_id").IsRequired();
            b.Property(x => x.OsaFormId).HasColumnName("osa_form_id").IsRequired();
            b.Property(x => x.Title).HasColumnName("title").HasMaxLength(500);
            b.Property(x => x.GosaWindowStart).HasColumnName("gosa_window_start").HasColumnType("date");
            b.Property(x => x.GosaWindowEnd).HasColumnName("gosa_window_end").HasColumnType("date");
            b.Property(x => x.ElectionYear).HasColumnName("election_year");
            b.Property(x => x.ShareholdersCount).HasColumnName("shareholders_count");
            b.Property(x => x.BoardMinNumber).HasColumnName("board_min_number");
            b.Property(x => x.BoardMemberNumber).HasColumnName("board_member_number");
            b.Property(x => x.ExecutiveDirectorsParticipate).HasColumnName("executive_directors_participate").HasDefaultValue(false);
            b.Property(x => x.ExecutiveDirectorsCount).HasColumnName("executive_directors_count");
            b.Property(x => x.NonExecutiveDirectorsParticipate).HasColumnName("non_executive_directors_participate").HasDefaultValue(false);
            b.Property(x => x.NonExecutiveDirectorsCount).HasColumnName("non_executive_directors_count");
            b.Property(x => x.IndependentDirectorsParticipate).HasColumnName("independent_directors_participate").HasDefaultValue(false);
            b.Property(x => x.IndependentDirectorsCount).HasColumnName("independent_directors_count");
            b.Property(x => x.ShareholdersListReceived).HasColumnName("shareholders_list_received").HasDefaultValue(false);
            b.Property(x => x.AbsenteeVoting).HasColumnName("absentee_voting").HasDefaultValue(false);
            b.Property(x => x.OsaHeld).HasColumnName("osa_held").HasDefaultValue(false);
            b.Property(x => x.ProtocolSigned).HasColumnName("protocol_signed").HasDefaultValue(false);
            b.Property(x => x.DeputyChairProvided).HasColumnName("deputy_chair_provided").HasDefaultValue(false);
            b.Property(x => x.SecretaryProvided).HasColumnName("secretary_provided").HasDefaultValue(true);
            b.Property(x => x.SecretarySignsProtocols).HasColumnName("secretary_signs_protocols").HasDefaultValue(false);
            b.Property(x => x.TemporaryChairProvided).HasColumnName("temporary_chair_provided").HasDefaultValue(false);
            b.Property(x => x.BoardCompositionApproved).HasColumnName("board_composition_approved").HasDefaultValue(false);
            b.Property(x => x.BoardMandatory).HasColumnName("board_mandatory").HasDefaultValue(false);
            b.Property(x => x.BoardApproved).HasColumnName("board_approved").HasDefaultValue(false);
            b.Property(x => x.TemporaryChairSelection).HasColumnName("temporary_chair_selection").HasMaxLength(50);
            b.Property(x => x.TemporaryChairName).HasColumnName("temporary_chair_name").HasMaxLength(300);
            b.Property(x => x.ProtocolSignedAt).HasColumnName("protocol_signed_at");
            b.Property(x => x.BallotDeadline).HasColumnName("ballot_deadline");
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            b.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("DRAFT");
            b.Property(x => x.FinalizedBy).HasColumnName("finalized_by");
            b.Property(x => x.FinalizedAt).HasColumnName("finalized_at");
            b.HasOne(x => x.RefOsaForm)
             .WithMany()
             .HasForeignKey(x => x.OsaFormId)
             .HasPrincipalKey(o => o.Id);
            b.HasOne(x => x.LegalEntity)
             .WithMany()
             .HasForeignKey(x => x.LegalEntityId);
        });

        modelBuilder.Entity<OsaMeetingFile>(b =>
        {
            b.ToTable("osa_meeting_files");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.OsaMeetingId).HasColumnName("osa_meeting_id").IsRequired();
            b.Property(x => x.FileId).HasColumnName("file_id").IsRequired();
            b.HasOne(x => x.OsaMeeting)
             .WithMany()
             .HasForeignKey(x => x.OsaMeetingId)
             .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.File)
             .WithMany()
             .HasForeignKey(x => x.FileId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BoardOfDirectors>(b =>
        {
            b.ToTable("board_of_directors");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.OsaMeetingId).HasColumnName("osa_meeting_id").IsRequired();
            b.Property(x => x.ElectionYear).HasColumnName("election_year");
            b.Property(x => x.StartedAt).HasColumnName("started_at");
            b.Property(x => x.EndedAt).HasColumnName("ended_at");
            b.Property(x => x.StatusId).HasColumnName("status_id").IsRequired();
            b.HasOne(x => x.OsaMeeting)
             .WithMany()
             .HasForeignKey(x => x.OsaMeetingId)
             .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Status)
             .WithMany()
             .HasForeignKey(x => x.StatusId);
        });

        modelBuilder.Entity<RefBoardOfDirectorsStatus>(b =>
        {
            b.ToTable("ref_board_of_directors_statuses");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Code).HasColumnName("code").HasMaxLength(20).IsRequired();
            b.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.Property(x => x.CreatedBy).HasColumnName("created_by").IsRequired();
            b.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<BoardMember>(b =>
        {
            b.ToTable("board_members");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.OsaMeetingId).HasColumnName("osa_meeting_id").IsRequired();
            b.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(300).IsRequired();
            b.Property(x => x.BoardMemberTypeId).HasColumnName("board_member_type_id");
            b.Property(x => x.Account).HasColumnName("account").HasMaxLength(100);
            b.Property(x => x.Email).HasColumnName("email").HasMaxLength(200);
            b.Property(x => x.UserId).HasColumnName("user_id");
            b.Property(x => x.BoardOfDirectorsId).HasColumnName("board_of_directors_id");
            b.HasOne(x => x.RefBoardMemberType)
             .WithMany()
             .HasForeignKey(x => x.BoardMemberTypeId);
            b.HasOne(x => x.OsaMeeting)
             .WithMany()
             .HasForeignKey(x => x.OsaMeetingId)
             .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.User)
             .WithMany()
             .HasForeignKey(x => x.UserId);
            b.HasOne(x => x.BoardOfDirectors)
             .WithMany()
             .HasForeignKey(x => x.BoardOfDirectorsId);
        });

        modelBuilder.Entity<RefBoardMemberType>(b =>
        {
            b.ToTable("ref_board_member_types");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Code).HasColumnName("code").HasMaxLength(20).IsRequired();
            b.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.Property(x => x.CreatedBy).HasColumnName("created_by").IsRequired();
            b.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<RefBoardRole>(b =>
        {
            b.ToTable("ref_board_roles");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Code).HasColumnName("code").HasMaxLength(20).IsRequired();
            b.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            b.Property(x => x.SortOrder).HasColumnName("sort_order");
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.Property(x => x.CreatedBy).HasColumnName("created_by").IsRequired();
            b.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<BoardMemberAppointment>(b =>
        {
            b.ToTable("board_member_appointments");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.BoardMemberId).HasColumnName("board_member_id").IsRequired();
            b.Property(x => x.RoleId).HasColumnName("role_id").IsRequired();
            b.Property(x => x.RoleCode).HasColumnName("role_code").HasMaxLength(20).IsRequired();
            b.Property(x => x.StartedAt).HasColumnName("started_at");
            b.Property(x => x.EndedAt).HasColumnName("ended_at");
            b.Property(x => x.StatusId).HasColumnName("status_id").IsRequired();
            b.Property(x => x.ResignedAt).HasColumnName("resigned_at");
            b.Property(x => x.ResignationReasonId).HasColumnName("resignation_reason_id");
            b.Property(x => x.LegalBasis).HasColumnName("legal_basis");
            b.HasOne(x => x.BoardMember)
             .WithMany()
             .HasForeignKey(x => x.BoardMemberId)
             .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Role)
             .WithMany()
             .HasForeignKey(x => x.RoleId);
            b.HasOne(x => x.Status)
             .WithMany()
             .HasForeignKey(x => x.StatusId);
            b.HasOne(x => x.RefResignationReason)
             .WithMany()
             .HasForeignKey(x => x.ResignationReasonId);
        });

        modelBuilder.Entity<ExtSparkCompany>(b =>
        {
            b.ToTable("ext_spark_company");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Inn).HasColumnName("inn").HasMaxLength(12).IsRequired();
            b.Property(x => x.Ogrn).HasColumnName("ogrn").HasMaxLength(15);
            b.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(500);
            b.Property(x => x.ShortName).HasColumnName("short_name").HasMaxLength(255);
            b.Property(x => x.OkopfCode).HasColumnName("okopf_code").HasMaxLength(10);
            b.Property(x => x.OkopfName).HasColumnName("okopf_name").HasMaxLength(255);
            b.Property(x => x.LegalAddress).HasColumnName("legal_address");
            b.Property(x => x.Status).HasColumnName("status").HasMaxLength(100);
            b.Property(x => x.RegistrationDate).HasColumnName("registration_date");
            b.Property(x => x.ShareholdersCount).HasColumnName("shareholders_count");
            b.Property(x => x.EmployeesCount).HasColumnName("employees_count");
            b.Property(x => x.FetchedAt).HasColumnName("fetched_at").IsRequired();
        });

        modelBuilder.Entity<ExtSparkManager>(b =>
        {
            b.ToTable("ext_spark_manager");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Inn).HasColumnName("inn").HasMaxLength(12).IsRequired();
            b.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(300).IsRequired();
            b.Property(x => x.Position).HasColumnName("position").HasMaxLength(200);
            b.Property(x => x.PersonInn).HasColumnName("person_inn").HasMaxLength(12);
            b.Property(x => x.StartDate).HasColumnName("start_date");
            b.Property(x => x.FetchedAt).HasColumnName("fetched_at").IsRequired();
        });

        modelBuilder.Entity<ExtSparkFounder>(b =>
        {
            b.ToTable("ext_spark_founder");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Inn).HasColumnName("inn").HasMaxLength(12).IsRequired();
            b.Property(x => x.Name).HasColumnName("name").HasMaxLength(500);
            b.Property(x => x.FounderInn).HasColumnName("founder_inn").HasMaxLength(12);
            b.Property(x => x.FounderOgrn).HasColumnName("founder_ogrn").HasMaxLength(15);
            b.Property(x => x.Country).HasColumnName("country").HasMaxLength(100);
            b.Property(x => x.IsForeign).HasColumnName("is_foreign").HasDefaultValue(false);
            b.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(300);
            b.Property(x => x.PersonInn).HasColumnName("person_inn").HasMaxLength(12);
            b.Property(x => x.Citizenship).HasColumnName("citizenship").HasMaxLength(100);
            b.Property(x => x.HeadOfOther).HasColumnName("head_of_other");
            b.Property(x => x.FounderOfOther).HasColumnName("founder_of_other");
            b.Property(x => x.IsEntrepreneur).HasColumnName("is_entrepreneur").HasDefaultValue(false);
            b.Property(x => x.Ogrnip).HasColumnName("ogrnip").HasMaxLength(15);
            b.Property(x => x.ShareAmount).HasColumnName("share_amount").HasColumnType("numeric(18,2)");
            b.Property(x => x.SharePercent).HasColumnName("share_percent").HasColumnType("numeric(5,2)");
            b.Property(x => x.EntryDate).HasColumnName("entry_date");
            b.Property(x => x.ExitDate).HasColumnName("exit_date");
            b.Property(x => x.FetchedAt).HasColumnName("fetched_at").IsRequired();
        });

        modelBuilder.Entity<BoardParticipant>(b =>
        {
            b.ToTable("board_participant");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.LegalEntityId).HasColumnName("legal_entity_id").IsRequired();
            b.Property(x => x.PersonId).HasColumnName("person_id");
            b.Property(x => x.ParticipantType).HasColumnName("participant_type").HasMaxLength(20).IsRequired().HasDefaultValue("FL");
            b.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(300);
            b.Property(x => x.PassportSeries).HasColumnName("passport_series").HasMaxLength(10);
            b.Property(x => x.PassportNumber).HasColumnName("passport_number").HasMaxLength(10);
            b.Property(x => x.PassportIssuedBy).HasColumnName("passport_issued_by").HasMaxLength(500);
            b.Property(x => x.PassportIssueDate).HasColumnName("passport_issue_date");
            b.Property(x => x.PassportDepartmentCode).HasColumnName("passport_department_code").HasMaxLength(10);
            b.Property(x => x.PassportRegistrationAddress).HasColumnName("passport_registration_address");
            b.Property(x => x.PersonInn).HasColumnName("person_inn").HasMaxLength(12);
            b.Property(x => x.Citizenship).HasColumnName("citizenship").HasMaxLength(100);
            b.Property(x => x.CompanyName).HasColumnName("company_name").HasMaxLength(500);
            b.Property(x => x.CompanyInn).HasColumnName("company_inn").HasMaxLength(12);
            b.Property(x => x.CompanyOgrn).HasColumnName("company_ogrn").HasMaxLength(15);
            b.Property(x => x.CompanyKpp).HasColumnName("company_kpp").HasMaxLength(9);
            b.Property(x => x.CompanyAddress).HasColumnName("company_address");
            b.Property(x => x.Ogrnip).HasColumnName("ogrnip").HasMaxLength(15);
            b.Property(x => x.SharePercent).HasColumnName("share_percent").HasColumnType("numeric(5,2)");
            b.Property(x => x.ShareAmount).HasColumnName("share_amount").HasColumnType("numeric(18,2)");
            b.Property(x => x.PaymentInfo).HasColumnName("payment_info").HasMaxLength(500);
            b.Property(x => x.ShareRegistrationInfo).HasColumnName("share_registration_info").HasMaxLength(500);
            b.Property(x => x.EntryDate).HasColumnName("entry_date");
            b.Property(x => x.ExitDate).HasColumnName("exit_date");
            b.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            b.Property(x => x.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            b.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
            b.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
            b.Property(x => x.CreatedBy).HasColumnName("created_by");
            b.HasIndex(x => x.LegalEntityId).HasDatabaseName("ix_board_participant_legal_entity");
        });

        modelBuilder.Entity<BoardTreasuryShare>(b =>
        {
            b.ToTable("board_treasury_share");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.LegalEntityId).HasColumnName("legal_entity_id").IsRequired();
            b.Property(x => x.SharePercent).HasColumnName("share_percent").HasColumnType("numeric(5,2)");
            b.Property(x => x.ShareAmount).HasColumnName("share_amount").HasColumnType("numeric(18,2)");
            b.Property(x => x.AcquiredDate).HasColumnName("acquired_date");
            b.Property(x => x.AcquisitionBasis).HasColumnName("acquisition_basis").HasMaxLength(500);
            b.Property(x => x.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            b.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
            b.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
            b.Property(x => x.CreatedBy).HasColumnName("created_by");
            b.HasIndex(x => x.LegalEntityId).HasDatabaseName("ix_board_treasury_share_legal_entity");
        });

        modelBuilder.Entity<BoardRegistryUpload>(b =>
        {
            b.ToTable("board_registry_upload");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.LegalEntityId).HasColumnName("legal_entity_id").IsRequired();
            b.Property(x => x.XmlFileId).HasColumnName("xml_file_id");
            b.Property(x => x.SignatureFileId).HasColumnName("signature_file_id");
            b.Property(x => x.XmlOriginalName).HasColumnName("xml_original_name").HasMaxLength(255);
            b.Property(x => x.SignatureOriginalName).HasColumnName("signature_original_name").HasMaxLength(255);
            b.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).IsRequired().HasDefaultValue("uploaded");
            b.Property(x => x.ParticipantCount).HasColumnName("participant_count");
            b.Property(x => x.UploadedBy).HasColumnName("uploaded_by");
            b.Property(x => x.UploadedAt).HasColumnName("uploaded_at").IsRequired();
            b.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
            b.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
            b.HasIndex(x => x.LegalEntityId).HasDatabaseName("ix_board_registry_upload_le");
        });

        modelBuilder.Entity<BoardParticipantChange>(b =>
        {
            b.ToTable("board_participant_change");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.LegalEntityId).HasColumnName("legal_entity_id").IsRequired();
            b.Property(x => x.ParticipantId).HasColumnName("participant_id").IsRequired();
            b.Property(x => x.ParticipantType).HasColumnName("participant_type").HasMaxLength(20).IsRequired();
            b.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(300);
            b.Property(x => x.PassportSeries).HasColumnName("passport_series").HasMaxLength(10);
            b.Property(x => x.PassportNumber).HasColumnName("passport_number").HasMaxLength(10);
            b.Property(x => x.PassportIssuedBy).HasColumnName("passport_issued_by").HasMaxLength(500);
            b.Property(x => x.PassportIssueDate).HasColumnName("passport_issue_date");
            b.Property(x => x.PassportDepartmentCode).HasColumnName("passport_department_code").HasMaxLength(10);
            b.Property(x => x.PassportRegistrationAddress).HasColumnName("passport_registration_address");
            b.Property(x => x.PersonInn).HasColumnName("person_inn").HasMaxLength(12);
            b.Property(x => x.Citizenship).HasColumnName("citizenship").HasMaxLength(100);
            b.Property(x => x.CompanyName).HasColumnName("company_name").HasMaxLength(500);
            b.Property(x => x.CompanyInn).HasColumnName("company_inn").HasMaxLength(12);
            b.Property(x => x.CompanyOgrn).HasColumnName("company_ogrn").HasMaxLength(15);
            b.Property(x => x.CompanyKpp).HasColumnName("company_kpp").HasMaxLength(9);
            b.Property(x => x.CompanyAddress).HasColumnName("company_address");
            b.Property(x => x.Ogrnip).HasColumnName("ogrnip").HasMaxLength(15);
            b.Property(x => x.SharePercent).HasColumnName("share_percent").HasColumnType("numeric(5,2)");
            b.Property(x => x.ShareAmount).HasColumnName("share_amount").HasColumnType("numeric(18,2)");
            b.Property(x => x.DocumentFileId).HasColumnName("document_file_id");
            b.Property(x => x.DocumentOriginalName).HasColumnName("document_original_name").HasMaxLength(255);
            b.Property(x => x.Source).HasColumnName("source").HasMaxLength(20);
            b.Property(x => x.Date).HasColumnName("date").HasMaxLength(50);
            b.Property(x => x.PaperDocNumber).HasColumnName("paper_doc_number").HasMaxLength(100);
            b.Property(x => x.Comment).HasColumnName("comment");
            b.Property(x => x.SubmittedBy).HasColumnName("submitted_by");
            b.Property(x => x.SubmittedAt).HasColumnName("submitted_at").IsRequired();
            b.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).IsRequired().HasDefaultValue("pending");
            b.Property(x => x.ReviewComment).HasColumnName("review_comment");
            b.Property(x => x.ReviewedBy).HasColumnName("reviewed_by");
            b.Property(x => x.ReviewedAt).HasColumnName("reviewed_at");
            b.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
            b.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
            b.HasIndex(x => x.LegalEntityId).HasDatabaseName("ix_board_participant_change_le");
            b.HasIndex(x => x.ParticipantId).HasDatabaseName("ix_board_participant_change_participant");
        });

        modelBuilder.Entity<ExtCbrFinOrgOrganization>(b =>
        {
            b.ToTable("ext_cbr_finorg_organization");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Inn).HasColumnName("inn").HasMaxLength(12).IsRequired();
            b.Property(x => x.CbrId).HasColumnName("cbr_id");
            b.Property(x => x.Ogrn).HasColumnName("ogrn").HasMaxLength(15);
            b.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(500);
            b.Property(x => x.ShortName).HasColumnName("short_name").HasMaxLength(255);
            b.Property(x => x.EngName).HasColumnName("eng_name").HasMaxLength(500);
            b.Property(x => x.Address).HasColumnName("address");
            b.Property(x => x.Phones).HasColumnName("phones").HasMaxLength(500);
            b.Property(x => x.Email).HasColumnName("email").HasMaxLength(255);
            b.Property(x => x.Okato).HasColumnName("okato");
            b.Property(x => x.Region).HasColumnName("region").HasMaxLength(255);
            b.Property(x => x.FoTypes).HasColumnName("fo_types").HasMaxLength(500);
            b.Property(x => x.Status).HasColumnName("status").HasMaxLength(50);
            b.Property(x => x.IsSroMember).HasColumnName("is_sro_member");
            b.Property(x => x.IsRss).HasColumnName("is_rss");
            b.Property(x => x.IsNpo).HasColumnName("is_npo");
            b.Property(x => x.IsAsv).HasColumnName("is_asv");
            b.Property(x => x.RegNumber).HasColumnName("reg_number");
            b.Property(x => x.Bic).HasColumnName("bic").HasMaxLength(20);
            b.Property(x => x.BankStatus).HasColumnName("bank_status").HasMaxLength(100);
            b.Property(x => x.RegistrationDate).HasColumnName("registration_date");
            b.Property(x => x.HasBranches).HasColumnName("has_branches");
            b.Property(x => x.FundValue).HasColumnName("fund_value").HasColumnType("numeric(18,2)");
            b.Property(x => x.WebSites).HasColumnName("web_sites").HasMaxLength(1000);
            b.Property(x => x.Error).HasColumnName("error");
            b.Property(x => x.FetchedAt).HasColumnName("fetched_at").IsRequired();
        });

        modelBuilder.Entity<ExtCbrFinOrgLicense>(b =>
        {
            b.ToTable("ext_cbr_finorg_license");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.OrganizationInn).HasColumnName("organization_inn").HasMaxLength(12).IsRequired();
            b.Property(x => x.VidId).HasColumnName("vid_id");
            b.Property(x => x.ActivityName).HasColumnName("activity_name").HasMaxLength(500);
            b.Property(x => x.Number).HasColumnName("number").HasMaxLength(100);
            b.Property(x => x.Name).HasColumnName("name").HasMaxLength(255);
            b.Property(x => x.StartDate).HasColumnName("start_date");
            b.Property(x => x.EndDate).HasColumnName("end_date");
            b.Property(x => x.FetchedAt).HasColumnName("fetched_at").IsRequired();
        });

        modelBuilder.Entity<TplOrgIntent>(b =>
        {
            b.ToTable("tpl_org_intents");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Code).HasColumnName("code").HasMaxLength(50);
            b.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(300);
            b.Property(x => x.Description).HasColumnName("description");
            b.Property(x => x.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            b.Property(x => x.IsForAo).HasColumnName("is_for_ao");
            b.Property(x => x.IsForLlc).HasColumnName("is_for_llc");
            b.Property(x => x.RequiresBoardOfDirectors).HasColumnName("requires_board_of_directors");
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.Property(x => x.CreatedBy).HasColumnName("created_by");
        });

        modelBuilder.Entity<TplOrgStage>(b =>
        {
            b.ToTable("tpl_org_stages");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.IntentId).HasColumnName("intent_id").IsRequired();
            b.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(300);
            b.Property(x => x.Description).HasColumnName("description");
            b.Property(x => x.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            b.Property(x => x.StartOffsetDays).HasColumnName("start_offset_days");
            b.Property(x => x.DeadlineRule).HasColumnName("deadline_rule").HasMaxLength(100);
            b.Property(x => x.DeadlineDays).HasColumnName("deadline_days");
            b.Property(x => x.MeasurementUnitId).HasColumnName("measurement_unit_id");
            b.HasOne(x => x.MeasurementUnit).WithMany().HasForeignKey(x => x.MeasurementUnitId).OnDelete(DeleteBehavior.Restrict);
            b.Property(x => x.DependencyType).HasColumnName("dependency_type").HasConversion<string>().HasDefaultValue(DependencyType.FS);
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.Property(x => x.CreatedBy).HasColumnName("created_by");
            b.HasOne(x => x.Intent).WithMany(x => x.Stages).HasForeignKey(x => x.IntentId);
            b.Property(x => x.PredecessorStageIds).HasColumnName("predecessor_stage_ids");
        });

        modelBuilder.Entity<TplOrgTaskOffer>(b =>
        {
            b.ToTable("tpl_org_offers");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.StageId).HasColumnName("stage_id").IsRequired();
            b.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(300);
            b.Property(x => x.Description).HasColumnName("description");
            b.Property(x => x.StartOffsetDays).HasColumnName("start_offset_days");
            b.Property(x => x.DeadlineRule).HasColumnName("deadline_rule").HasMaxLength(100);
            b.Property(x => x.DeadlineDays).HasColumnName("deadline_days");
            b.Property(x => x.MeasurementUnitId).HasColumnName("measurement_unit_id");
            b.HasOne(x => x.MeasurementUnit).WithMany().HasForeignKey(x => x.MeasurementUnitId).OnDelete(DeleteBehavior.Restrict);
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.Property(x => x.CreatedBy).HasColumnName("created_by");
            b.HasOne(x => x.Stage).WithMany(x => x.Offers).HasForeignKey(x => x.StageId);
            b.Property(x => x.AssignedRoleId).HasColumnName("assigned_role_id");
            b.Property(x => x.AssignedBoardRoleId).HasColumnName("assigned_board_role_id");
            b.Property(x => x.RequireNotaryConfirmation).HasColumnName("require_notary_confirmation");
            b.Property(x => x.RequireAllSignConfirmation).HasColumnName("require_all_sign_confirmation");
            b.Property(x => x.RequireCommittees).HasColumnName("require_committees");
            b.Property(x => x.RequireBoardRegulation).HasColumnName("require_board_regulation");
            b.Property(x => x.RequireCustomCharter).HasColumnName("require_custom_charter");
            b.Property(x => x.RequireExecutiveBodyA).HasColumnName("require_executive_body_a");
            b.Property(x => x.RequireBoardOfDirectors).HasColumnName("require_board_of_directors");
            b.Property(x => x.RequireDocumentFlowLegalElectronic).HasColumnName("require_document_flow_legal_electronic");
            b.Property(x => x.RequireMandatoryAudit).HasColumnName("require_mandatory_audit");
            b.Property(x => x.RequireRevisionCommission).HasColumnName("require_revision_commission");
            b.Property(x => x.DependencyType).HasColumnName("dependency_type").HasConversion<string>().HasDefaultValue(DependencyType.FS);
            b.Property(x => x.PredecessorOfferIds).HasColumnName("predecessor_offer_ids");
            b.HasOne(x => x.AssignedRole).WithMany().HasForeignKey(x => x.AssignedRoleId);
            b.HasOne(x => x.AssignedBoardRole).WithMany().HasForeignKey(x => x.AssignedBoardRoleId);
        });

        modelBuilder.Entity<TplOrgMilestone>(b =>
        {
            b.ToTable("tpl_org_milestones");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.IntentId).HasColumnName("intent_id").IsRequired();
            b.Property(x => x.StageId).HasColumnName("stage_id");
            b.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(300);
            b.Property(x => x.Description).HasColumnName("description");
            b.Property(x => x.MilestoneType).HasColumnName("milestone_type").HasConversion<string>().IsRequired();
            b.Property(x => x.PredecessorOfferIds).HasColumnName("predecessor_offer_ids");
            b.Property(x => x.PredecessorStageIds).HasColumnName("predecessor_stage_ids");
            b.Property(x => x.OffsetDays).HasColumnName("offset_days");
            b.Property(x => x.MeasurementUnitId).HasColumnName("measurement_unit_id");
            b.HasOne(x => x.MeasurementUnit).WithMany().HasForeignKey(x => x.MeasurementUnitId).OnDelete(DeleteBehavior.Restrict);
            b.Property(x => x.ControlOfferId).HasColumnName("control_offer_id");
            b.HasOne(x => x.ControlOffer).WithMany().HasForeignKey(x => x.ControlOfferId).OnDelete(DeleteBehavior.Restrict);
            b.Property(x => x.LegalReference).HasColumnName("legal_reference").HasMaxLength(500);
            b.Property(x => x.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.Property(x => x.CreatedBy).HasColumnName("created_by");
            b.HasOne(x => x.Intent).WithMany().HasForeignKey(x => x.IntentId);
            b.HasOne(x => x.Stage).WithMany(x => x.Milestones).HasForeignKey(x => x.StageId);
        });

        modelBuilder.Entity<OrgIntent>(b =>
        {
            b.ToTable("org_intents");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.LegalEntityId).HasColumnName("legal_entity_id").IsRequired();
            b.Property(x => x.TemplateIntentId).HasColumnName("template_intent_id");
            b.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(300);
            b.Property(x => x.Description).HasColumnName("description");
            b.Property(x => x.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            b.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("PLANNED");
            b.Property(x => x.ActualStart).HasColumnName("actual_start").HasColumnType("date");
            b.Property(x => x.ActualEnd).HasColumnName("actual_end").HasColumnType("date");
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.HasOne(x => x.LegalEntity).WithMany().HasForeignKey(x => x.LegalEntityId);
            b.HasOne(x => x.TemplateIntent).WithMany().HasForeignKey(x => x.TemplateIntentId);
        });

        modelBuilder.Entity<OrgStage>(b =>
        {
            b.ToTable("org_stages");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.IntentId).HasColumnName("intent_id").IsRequired();
            b.Property(x => x.TemplateStageId).HasColumnName("template_stage_id");
            b.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(300);
            b.Property(x => x.Description).HasColumnName("description");
            b.Property(x => x.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            b.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("PLANNED");
            b.Property(x => x.ActualStart).HasColumnName("actual_start").HasColumnType("date");
            b.Property(x => x.ActualEnd).HasColumnName("actual_end").HasColumnType("date");
            b.Property(x => x.PlannedStart).HasColumnName("planned_start").HasColumnType("date");
            b.Property(x => x.PlannedEnd).HasColumnName("planned_end").HasColumnType("date");
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.HasOne(x => x.Intent).WithMany(x => x.Stages).HasForeignKey(x => x.IntentId);
            b.HasOne(x => x.TemplateStage).WithMany().HasForeignKey(x => x.TemplateStageId);
            b.Property(x => x.DependencyType).HasColumnName("dependency_type").HasConversion<string>().HasDefaultValue(DependencyType.FS);
            b.Property(x => x.PredecessorStageIds).HasColumnName("predecessor_stage_ids");
        });

        modelBuilder.Entity<OrgTask>(b =>
        {
            b.ToTable("org_tasks");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.StageId).HasColumnName("stage_id").IsRequired();
            b.Property(x => x.TemplateOfferId).HasColumnName("template_offer_id");
            b.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(300);
            b.Property(x => x.Description).HasColumnName("description");
            b.Property(x => x.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            b.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("PLANNED");
            b.Property(x => x.AssignedUserId).HasColumnName("assigned_user_id");
            b.Property(x => x.AssignedRoleId).HasColumnName("assigned_role_id");
            b.Property(x => x.AssignedBoardRoleId).HasColumnName("assigned_board_role_id");
            b.Property(x => x.CandidateRoles).HasColumnName("candidate_roles");
            b.Property(x => x.PredecessorTaskIds).HasColumnName("predecessor_task_ids");
            b.Property(x => x.DependencyType).HasColumnName("dependency_type").HasConversion<string>().HasDefaultValue(DependencyType.FS);
            b.Property(x => x.ActualStart).HasColumnName("actual_start").HasColumnType("date");
            b.Property(x => x.ActualEnd).HasColumnName("actual_end").HasColumnType("date");
            b.Property(x => x.PlannedStart).HasColumnName("planned_start").HasColumnType("date");
            b.Property(x => x.PlannedEnd).HasColumnName("planned_end").HasColumnType("date");
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.HasOne(x => x.Stage).WithMany(x => x.Tasks).HasForeignKey(x => x.StageId);
            b.HasOne(x => x.TemplateOffer).WithMany().HasForeignKey(x => x.TemplateOfferId);
            b.HasOne(x => x.AssignedUser).WithMany().HasForeignKey(x => x.AssignedUserId);
            b.HasOne(x => x.AssignedRole).WithMany().HasForeignKey(x => x.AssignedRoleId);
            b.HasOne(x => x.AssignedBoardRole).WithMany().HasForeignKey(x => x.AssignedBoardRoleId);
        });

        modelBuilder.Entity<OrgMilestone>(b =>
        {
            b.ToTable("org_milestones");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.IntentId).HasColumnName("intent_id").IsRequired();
            b.Property(x => x.TemplateMilestoneId).HasColumnName("template_milestone_id");
            b.Property(x => x.StageId).HasColumnName("stage_id");
            b.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(300);
            b.Property(x => x.Description).HasColumnName("description");
            b.Property(x => x.MilestoneType).HasColumnName("milestone_type").HasConversion<string>().IsRequired();
            b.Property(x => x.PredecessorTaskIds).HasColumnName("predecessor_task_ids");
            b.Property(x => x.PredecessorStageIds).HasColumnName("predecessor_stage_ids");
            b.Property(x => x.PlannedDate).HasColumnName("planned_date").HasColumnType("date");
            b.Property(x => x.ActualDate).HasColumnName("actual_date").HasColumnType("date");
            b.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("PLANNED");
            b.Property(x => x.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.HasOne(x => x.Intent).WithMany().HasForeignKey(x => x.IntentId);
            b.HasOne(x => x.TemplateMilestone).WithMany().HasForeignKey(x => x.TemplateMilestoneId);
            b.HasOne(x => x.Stage).WithMany().HasForeignKey(x => x.StageId);
        });


        modelBuilder.Entity<TplOrgOfferRole>(b =>
        {
            b.ToTable("tpl_org_offer_roles");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.TplOfferId).HasColumnName("tpl_offer_id").IsRequired();
            b.Property(x => x.RoleId).HasColumnName("role_id").IsRequired();
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.HasOne(x => x.Offer).WithMany(x => x.OfferRoles).HasForeignKey(x => x.TplOfferId);
            b.HasOne(x => x.Role).WithMany().HasForeignKey(x => x.RoleId);
        });

        modelBuilder.Entity<RefBoardMemberAppointmentStatus>(b =>
        {
            b.ToTable("ref_board_member_appointment_statuses");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Code).HasColumnName("code").HasMaxLength(20).IsRequired();
            b.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.Property(x => x.CreatedBy).HasColumnName("created_by").IsRequired();
            b.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<RefResignationReason>(b =>
        {
            b.ToTable("ref_resignation_reasons");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Code).HasColumnName("code").HasMaxLength(20).IsRequired();
            b.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.Property(x => x.CreatedBy).HasColumnName("created_by").IsRequired();
            b.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<UserBoardMemberResignation>(b =>
        {
            b.ToTable("user_board_member_resignations");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
            b.Property(x => x.BoardMemberAppointmentId).HasColumnName("board_member_appointment_id").IsRequired();
            b.Property(x => x.ResignedAt).HasColumnName("resigned_at").IsRequired();
            b.Property(x => x.ResignationReasonId).HasColumnName("resignation_reason_id").IsRequired();
            b.Property(x => x.RdlExtractFileId).HasColumnName("rdl_extract_file_id");
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.HasOne(x => x.User)
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.BoardMemberAppointment)
             .WithMany()
             .HasForeignKey(x => x.BoardMemberAppointmentId)
             .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.RefResignationReason)
             .WithMany()
             .HasForeignKey(x => x.ResignationReasonId);
        });

        modelBuilder.Entity<AgendaItem>(b =>
        {
            b.ToTable("agenda_items");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.BoardOfDirectorsId).HasColumnName("board_of_directors_id").IsRequired();
            b.Property(x => x.LegalEntityId).HasColumnName("legal_entity_id");
            b.Property(x => x.ShareRequestId).HasColumnName("share_request_id");
            b.Property(x => x.Title).HasColumnName("title").IsRequired();
            b.Property(x => x.TargetType).HasColumnName("target_type").HasMaxLength(20).IsRequired();
            b.Property(x => x.Reason).HasColumnName("reason").IsRequired();
            b.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("PENDING");
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.HasOne(x => x.ShareRequest)
             .WithMany()
             .HasForeignKey(x => x.ShareRequestId);
        });

        modelBuilder.Entity<AgendaProposal>(b =>
        {
            b.ToTable("agenda_proposals");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.SubmitterName).HasColumnName("submitter_name").IsRequired().HasMaxLength(300);
            b.Property(x => x.SubmitterEmail).HasColumnName("submitter_email").HasMaxLength(300);
            b.Property(x => x.ProposalText).HasColumnName("proposal_text").IsRequired();
            b.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("SUBMITTED");
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<ElectionProposal>(b =>
        {
            b.ToTable("election_proposals");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.BoardOfDirectorsId).HasColumnName("board_of_directors_id").IsRequired();
            b.Property(x => x.Position).HasColumnName("position").HasMaxLength(20).IsRequired();
            b.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("OPEN");
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<ElectionCandidacy>(b =>
        {
            b.ToTable("election_candidacies");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.ProposalId).HasColumnName("proposal_id").IsRequired();
            b.Property(x => x.CandidateMemberId).HasColumnName("candidate_member_id").IsRequired();
            b.Property(x => x.ConfirmedByMemberId).HasColumnName("confirmed_by_member_id");
            b.Property(x => x.ConfirmedAt).HasColumnName("confirmed_at");
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.HasOne(x => x.Proposal).WithMany(x => x.Candidacies).HasForeignKey(x => x.ProposalId);
            b.HasOne(x => x.CandidateMember).WithMany().HasForeignKey(x => x.CandidateMemberId);
            b.HasOne(x => x.ConfirmedByMember).WithMany().HasForeignKey(x => x.ConfirmedByMemberId);
        });

        modelBuilder.Entity<SystemSetting>(b =>
        {
            b.ToTable("system_settings");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Key).HasColumnName("key").IsRequired().HasMaxLength(100);
            b.Property(x => x.Value).HasColumnName("value");
            b.Property(x => x.Description).HasColumnName("description");
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            b.HasIndex(x => x.Key).IsUnique();
        });

        modelBuilder.Entity<ShareRequestSupport>(b =>
        {
            b.ToTable("share_request_support");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.ShareRequestId).HasColumnName("share_request_id").IsRequired();
            b.Property(x => x.ParticipantId).HasColumnName("participant_id").IsRequired();
            b.Property(x => x.SharePercentAtSupport).HasColumnName("share_percent_at_support").HasColumnType("numeric(6,2)").IsRequired();
            b.Property(x => x.SupportedAt).HasColumnName("supported_at").IsRequired().HasDefaultValueSql("NOW()");
            b.Property(x => x.WithdrawnAt).HasColumnName("withdrawn_at");
            b.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired().HasDefaultValueSql("NOW()");
            b.HasOne(x => x.ShareRequest).WithMany(r => r.Supports).HasForeignKey(x => x.ShareRequestId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Participant).WithMany().HasForeignKey(x => x.ParticipantId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => x.ShareRequestId).HasDatabaseName("ix_srs_request");
            b.HasIndex(x => x.ParticipantId).HasDatabaseName("ix_srs_participant");
        });

        modelBuilder.Entity<ShareRequestFile>(b =>
        {
            b.ToTable("share_request_files");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.ShareRequestId).HasColumnName("share_request_id").IsRequired();
            b.Property(x => x.FileId).HasColumnName("file_id").IsRequired();
            b.HasOne(x => x.ShareRequest).WithMany().HasForeignKey(x => x.ShareRequestId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.File).WithMany().HasForeignKey(x => x.FileId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => x.ShareRequestId).HasDatabaseName("ix_srf_request");
            b.HasIndex(x => x.FileId).HasDatabaseName("ix_srf_file");
        });

        modelBuilder.Entity<RefDocumentType>(b =>
        {
            b.ToTable("ref_document_type");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
            b.Property(x => x.Name).HasColumnName("name").HasMaxLength(300).IsRequired();
            b.Property(x => x.IsUnitary).HasColumnName("is_unitary").HasDefaultValue(false);
            b.Property(x => x.StorageYears).HasColumnName("storage_years").HasDefaultValue(3);
            b.Property(x => x.IsForLlc).HasColumnName("is_for_llc").HasDefaultValue(false);
            b.Property(x => x.IsForNjsc).HasColumnName("is_for_njsc").HasDefaultValue(false);
            b.Property(x => x.IsForPjsc).HasColumnName("is_for_pjsc").HasDefaultValue(false);
            b.Property(x => x.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            b.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ux_ref_document_type_code");
        });

        modelBuilder.Entity<RefDocumentAccessMethod>(b =>
        {
            b.ToTable("ref_document_access_method");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
            b.Property(x => x.Name).HasColumnName("name").HasMaxLength(300).IsRequired();
            b.Property(x => x.Description).HasColumnName("description");
            b.Property(x => x.DeadlineDays).HasColumnName("deadline_days");
            b.Property(x => x.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            b.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ux_ref_access_method_code");
        });

        modelBuilder.Entity<RefDocumentRefusalReason>(b =>
        {
            b.ToTable("ref_document_refusal_reason");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
            b.Property(x => x.Name).HasColumnName("name").HasMaxLength(300).IsRequired();
            b.Property(x => x.Description).HasColumnName("description");
            b.Property(x => x.LegalBasis).HasColumnName("legal_basis").HasMaxLength(300);
            b.Property(x => x.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            b.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ux_ref_refusal_reason_code");
        });

    }
}
