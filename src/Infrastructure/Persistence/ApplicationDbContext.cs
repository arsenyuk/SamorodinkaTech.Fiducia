using Microsoft.EntityFrameworkCore;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Enums;
using SamorodinkaTech.Fiducia.Domain.Interfaces;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence;

public class FiduciaDbContext : Microsoft.EntityFrameworkCore.DbContext, IApplicationDbContext
{
    public FiduciaDbContext(DbContextOptions<FiduciaDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Committee> Committees => Set<Committee>();
    public DbSet<CommitteeMember> CommitteeMembers => Set<CommitteeMember>();
    public DbSet<Meeting> Meetings => Set<Meeting>();
    public DbSet<AgendaQuestion> AgendaQuestions => Set<AgendaQuestion>();
    public DbSet<CommitteeTask> CommitteeTasks => Set<CommitteeTask>();
    public DbSet<Bulletin> Bulletins => Set<Bulletin>();
    public DbSet<SecurityAuditLog> SecurityAuditLogs => Set<SecurityAuditLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Okopf> Okopf => Set<Okopf>();
    public DbSet<RefMonth> RefMonths => Set<RefMonth>();
    public DbSet<OsaForm> OsaForms => Set<OsaForm>();
    public DbSet<OsaMeeting> OsaMeetings => Set<OsaMeeting>();
    public DbSet<BoardMember> BoardMembers => Set<BoardMember>();
    public DbSet<BoardOfDirectors> BoardsOfDirectors => Set<BoardOfDirectors>();
    public DbSet<BoardOfDirectorsStatus> BoardOfDirectorsStatuses => Set<BoardOfDirectorsStatus>();
    public DbSet<BoardMemberType> BoardMemberTypes => Set<BoardMemberType>();
    public DbSet<BoardRole> BoardRoles => Set<BoardRole>();
    public DbSet<BoardMemberAppointment> BoardMemberAppointments => Set<BoardMemberAppointment>();
    public DbSet<BoardMemberAppointmentStatus> BoardMemberAppointmentStatuses => Set<BoardMemberAppointmentStatus>();
    public DbSet<ResignationReason> ResignationReasons => Set<ResignationReason>();
    public DbSet<UserBoardMemberResignation> UserBoardMemberResignations => Set<UserBoardMemberResignation>();
    public DbSet<OsaMeetingFile> OsaMeetingFiles => Set<OsaMeetingFile>();
    public DbSet<LegalEntity> LegalEntities => Set<LegalEntity>();
    public DbSet<CurrentWorkplace> CurrentWorkplaces => Set<CurrentWorkplace>();
    public DbSet<LegalEntityBoardSettings> LegalEntityBoardSettings => Set<LegalEntityBoardSettings>();
    public DbSet<LegalEntityVotingRules> LegalEntityVotingRules => Set<LegalEntityVotingRules>();
    public DbSet<FileEntry> Files => Set<FileEntry>();
    public DbSet<ExtSparkCompany> ExtSparkCompanies => Set<ExtSparkCompany>();
    public DbSet<RefMeetingForm> MeetingForms => Set<RefMeetingForm>();
    public DbSet<ExtSparkManager> ExtSparkManagers => Set<ExtSparkManager>();


    public DbSet<TplOrgIntent> TplOrgIntents => Set<TplOrgIntent>();
    public DbSet<TplOrgStage> TplOrgStages => Set<TplOrgStage>();
    public DbSet<TplOrgOffer> TplOrgOffers => Set<TplOrgOffer>();
    public DbSet<TplOrgTask> TplOrgTasks => Set<TplOrgTask>();
    public DbSet<OrgIntent> OrgIntents => Set<OrgIntent>();
    public DbSet<OrgStage> OrgStages => Set<OrgStage>();
    public DbSet<OrgOffer> OrgOffers => Set<OrgOffer>();
    public DbSet<OrgTask> OrgTasks => Set<OrgTask>();

    public DbSet<TplOrgOfferRole> TplOrgOfferRoles => Set<TplOrgOfferRole>();
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
            b.HasIndex(x => x.Name).HasDatabaseName("ix_legal_entities_name");
            b.HasIndex(x => x.Inn).HasDatabaseName("ix_legal_entities_inn");
            b.HasIndex(x => x.Ogrn).HasDatabaseName("ix_legal_entities_ogrn");
            b.HasOne(x => x.Okopf)
             .WithMany()
             .HasForeignKey(x => x.OkopfId)
             .HasPrincipalKey(o => o.Id);
        });

        modelBuilder.Entity<RefMonth>(b =>
        {
            b.ToTable("ref_month");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Code).HasColumnName("code").HasMaxLength(2).IsRequired();
            b.Property(x => x.Name).HasColumnName("name").HasMaxLength(20).IsRequired();
            b.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ux_ref_month_code");
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
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.HasOne(x => x.LegalEntity)
             .WithMany()
             .HasForeignKey(x => x.LegalEntityId);
        });

        modelBuilder.Entity<RefMeetingForm>(b =>
        {
            b.ToTable("ref_meeting_form");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Code).HasColumnName("code").HasMaxLength(10).IsRequired();
            b.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            b.Property(x => x.ShortName).HasColumnName("short_name").HasMaxLength(50);
            b.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ux_ref_meeting_form_code");
        });

        modelBuilder.Entity<OsaForm>(b =>
        {
            b.ToTable("ref_osa_form");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Code).HasColumnName("code").HasMaxLength(10).IsRequired();
            b.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            b.Property(x => x.ShortName).HasColumnName("short_name").HasMaxLength(50);
            b.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ux_ref_osa_form_code");
        });

        modelBuilder.Entity<OsaMeeting>(b =>
        {
            b.ToTable("osa_meetings");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.OsaFormId).HasColumnName("osa_form_id").IsRequired();
            b.Property(x => x.Title).HasColumnName("title").HasMaxLength(500);
            b.Property(x => x.GosaWindowStart).HasColumnName("gosa_window_start").HasColumnType("date");
            b.Property(x => x.GosaWindowEnd).HasColumnName("gosa_window_end").HasColumnType("date");
            b.Property(x => x.GosaYear).HasColumnName("gosa_year");
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
            b.HasOne(x => x.OsaForm)
             .WithMany()
             .HasForeignKey(x => x.OsaFormId)
             .HasPrincipalKey(o => o.Id);
        });

        modelBuilder.Entity<OsaMeetingFile>(b =>
        {
            b.ToTable("osa_meeting_files");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.OsaMeetingId).HasColumnName("osa_meeting_id").IsRequired();
            b.Property(x => x.FileId).HasColumnName("file_id").IsRequired();
            b.Property(x => x.FileType).HasColumnName("file_type").HasMaxLength(50).IsRequired();
            b.Property(x => x.DisplayName).HasColumnName("display_name").HasMaxLength(255);
            b.HasOne(x => x.OsaMeeting)
             .WithMany()
             .HasForeignKey(x => x.OsaMeetingId)
             .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.File)
             .WithMany()
             .HasForeignKey(x => x.FileId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BoardOfDirectors>(b =>
        {
            b.ToTable("board_of_directors");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.OsaMeetingId).HasColumnName("osa_meeting_id").IsRequired();
            b.Property(x => x.GosaYear).HasColumnName("gosa_year");
            b.Property(x => x.StartedAt).HasColumnName("started_at");
            b.Property(x => x.EndedAt).HasColumnName("ended_at");
            b.Property(x => x.StatusId).HasColumnName("status_id").IsRequired();
            b.HasOne(x => x.OsaMeeting)
             .WithMany()
             .HasForeignKey(x => x.OsaMeetingId)
             .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.Status)
             .WithMany()
             .HasForeignKey(x => x.StatusId);
        });

        modelBuilder.Entity<BoardOfDirectorsStatus>(b =>
        {
            b.ToTable("ref_board_of_directors_statuses");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Code).HasColumnName("code").HasMaxLength(20);
            b.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
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
            b.HasOne(x => x.BoardMemberType)
             .WithMany()
             .HasForeignKey(x => x.BoardMemberTypeId);
            b.HasOne(x => x.OsaMeeting)
             .WithMany()
             .HasForeignKey(x => x.OsaMeetingId)
             .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.User)
             .WithMany()
             .HasForeignKey(x => x.UserId);
            b.HasOne(x => x.BoardOfDirectors)
             .WithMany()
             .HasForeignKey(x => x.BoardOfDirectorsId);
        });

        modelBuilder.Entity<BoardMemberType>(b =>
        {
            b.ToTable("ref_board_member_types");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Code).HasColumnName("code").HasMaxLength(20);
            b.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            b.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<BoardRole>(b =>
        {
            b.ToTable("ref_board_roles");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Code).HasColumnName("code").HasMaxLength(20);
            b.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            b.Property(x => x.SortOrder).HasColumnName("sort_order");
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
             .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.Role)
             .WithMany()
             .HasForeignKey(x => x.RoleId);
            b.HasOne(x => x.Status)
             .WithMany()
             .HasForeignKey(x => x.StatusId);
            b.HasOne(x => x.ResignationReason)
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

        modelBuilder.Entity<TplOrgIntent>(b =>
        {
            b.ToTable("tpl_org_intents");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Code).HasColumnName("code").HasMaxLength(50);
            b.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(300);
            b.Property(x => x.Description).HasColumnName("description");
            b.Property(x => x.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
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
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.HasOne(x => x.Intent).WithMany(x => x.Stages).HasForeignKey(x => x.IntentId);
        });

        modelBuilder.Entity<TplOrgOffer>(b =>
        {
            b.ToTable("tpl_org_offers");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.StageId).HasColumnName("stage_id").IsRequired();
            b.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(300);
            b.Property(x => x.Description).HasColumnName("description");
            b.Property(x => x.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            b.Property(x => x.StartOffsetDays).HasColumnName("start_offset_days");
            b.Property(x => x.DeadlineRule).HasColumnName("deadline_rule").HasMaxLength(100);
            b.Property(x => x.DeadlineDays).HasColumnName("deadline_days");
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.HasOne(x => x.Stage).WithMany(x => x.Offers).HasForeignKey(x => x.StageId);
        });

        modelBuilder.Entity<TplOrgTask>(b =>
        {
            b.ToTable("tpl_org_tasks");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.OfferId).HasColumnName("offer_id").IsRequired();
            b.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(300);
            b.Property(x => x.Description).HasColumnName("description");
            b.Property(x => x.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            b.Property(x => x.AssignedRoleId).HasColumnName("assigned_role_id");
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.HasOne(x => x.Offer).WithMany(x => x.Tasks).HasForeignKey(x => x.OfferId);
            b.HasOne(x => x.AssignedRole).WithMany().HasForeignKey(x => x.AssignedRoleId);
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
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.HasOne(x => x.Intent).WithMany(x => x.Stages).HasForeignKey(x => x.IntentId);
            b.HasOne(x => x.TemplateStage).WithMany().HasForeignKey(x => x.TemplateStageId);
        });

        modelBuilder.Entity<OrgOffer>(b =>
        {
            b.ToTable("org_offers");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.StageId).HasColumnName("stage_id").IsRequired();
            b.Property(x => x.TemplateOfferId).HasColumnName("template_offer_id");
            b.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(300);
            b.Property(x => x.Description).HasColumnName("description");
            b.Property(x => x.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            b.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("PLANNED");
            b.Property(x => x.AssignedUserId).HasColumnName("assigned_user_id");
            b.Property(x => x.CandidateRoles).HasColumnName("candidate_roles");
            b.Property(x => x.ActualStart).HasColumnName("actual_start").HasColumnType("date");
            b.Property(x => x.ActualEnd).HasColumnName("actual_end").HasColumnType("date");
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.HasOne(x => x.Stage).WithMany(x => x.Offers).HasForeignKey(x => x.StageId);
            b.HasOne(x => x.TemplateOffer).WithMany().HasForeignKey(x => x.TemplateOfferId);
            b.HasOne(x => x.AssignedUser).WithMany().HasForeignKey(x => x.AssignedUserId);
        });

        modelBuilder.Entity<OrgTask>(b =>
        {
            b.ToTable("org_tasks");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.OfferId).HasColumnName("offer_id").IsRequired();
            b.Property(x => x.TemplateTaskId).HasColumnName("template_task_id");
            b.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(300);
            b.Property(x => x.Description).HasColumnName("description");
            b.Property(x => x.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            b.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("PLANNED");
            b.Property(x => x.AssignedUserId).HasColumnName("assigned_user_id");
            b.Property(x => x.AssignedRoleId).HasColumnName("assigned_role_id");
            b.Property(x => x.ActualStart).HasColumnName("actual_start").HasColumnType("date");
            b.Property(x => x.ActualEnd).HasColumnName("actual_end").HasColumnType("date");
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.HasOne(x => x.Offer).WithMany(x => x.Tasks).HasForeignKey(x => x.OfferId);
            b.HasOne(x => x.TemplateTask).WithMany().HasForeignKey(x => x.TemplateTaskId);
            b.HasOne(x => x.AssignedUser).WithMany().HasForeignKey(x => x.AssignedUserId);
            b.HasOne(x => x.AssignedRole).WithMany().HasForeignKey(x => x.AssignedRoleId);
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

        modelBuilder.Entity<BoardMemberAppointmentStatus>(b =>
        {
            b.ToTable("ref_board_member_appointment_statuses");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Code).HasColumnName("code").HasMaxLength(20);
            b.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            b.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<ResignationReason>(b =>
        {
            b.ToTable("ref_resignation_reasons");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Code).HasColumnName("code").HasMaxLength(20);
            b.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
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
             .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.BoardMemberAppointment)
             .WithMany()
             .HasForeignKey(x => x.BoardMemberAppointmentId)
             .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.ResignationReason)
             .WithMany()
             .HasForeignKey(x => x.ResignationReasonId);
        });


    }
}
