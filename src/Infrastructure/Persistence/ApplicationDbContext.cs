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
    public DbSet<BoardMemberType> BoardMemberTypes => Set<BoardMemberType>();
    public DbSet<BoardRole> BoardRoles => Set<BoardRole>();
    public DbSet<BoardMemberAppointment> BoardMemberAppointments => Set<BoardMemberAppointment>();
    public DbSet<OsaMeetingFile> OsaMeetingFiles => Set<OsaMeetingFile>();
    public DbSet<LegalEntity> LegalEntities => Set<LegalEntity>();
    public DbSet<CurrentWorkplace> CurrentWorkplaces => Set<CurrentWorkplace>();
    public DbSet<LegalEntityBoardSettings> LegalEntityBoardSettings => Set<LegalEntityBoardSettings>();
    public DbSet<FileEntry> Files => Set<FileEntry>();

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
            b.Property(x => x.ShareholdersCount).HasColumnName("shareholders_count");
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

        modelBuilder.Entity<Okopf>(b =>
        {
            b.ToTable("ref_okopf");
            b.HasKey(o => o.Id);
            b.Property(o => o.Id).HasColumnName("id");
            b.Property(o => o.Code).HasColumnName("code").HasMaxLength(10);
            b.Property(o => o.Name).HasColumnName("name").IsRequired();
            b.HasIndex(o => o.Code).IsUnique().HasDatabaseName("ux_ref_okopf_code");
        });

        modelBuilder.Entity<CurrentWorkplace>(b =>
        {
            b.ToTable("current_workplace");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.FullName).HasColumnName("full_name").IsRequired();
            b.Property(x => x.Position).HasColumnName("position").HasMaxLength(200);
        });

        modelBuilder.Entity<LegalEntityBoardSettings>(b =>
        {
            b.ToTable("legal_entity_board_settings");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.MinimumMemberNumber).HasColumnName("minimum_member_number");
            b.Property(x => x.MemberNumber).HasColumnName("member_number");
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
            b.Property(x => x.BoardMandatory).HasColumnName("board_mandatory").HasDefaultValue(false);
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
            b.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("ACTIVE");
            b.HasOne(x => x.BoardMember)
             .WithMany()
             .HasForeignKey(x => x.BoardMemberId)
             .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.Role)
             .WithMany()
             .HasForeignKey(x => x.RoleId);
        });
    }
}
