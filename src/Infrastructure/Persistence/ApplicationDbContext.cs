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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FiduciaDbContext).Assembly);

        // Конфигурация LegalEntity → ref_okopf по UUID

        modelBuilder.Entity<TplOrgIntent>(b =>
        {
            b.ToTable("tpl_org_intents");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
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
            b.Property(x => x.CandidateRoles).HasColumnName("candidate_roles");
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


    }
}
