using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Enums;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class CommitteeConfiguration : IEntityTypeConfiguration<Committee>
{
    public void Configure(EntityTypeBuilder<Committee> builder)
    {
        builder.ToTable("committees");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.Code).HasColumnName("code").HasMaxLength(20).IsRequired();
        builder.Property(c => c.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
        builder.Property(c => c.BehaviorType)
            .HasColumnName("behavior_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(c => c.Description).HasColumnName("description");
        builder.Property(c => c.IsMandatoryForPublic).HasColumnName("is_mandatory_for_public").HasDefaultValue(false);
        builder.Property(c => c.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(c => c.ChairId).HasColumnName("chair_id");
        builder.Property(c => c.SecretaryId).HasColumnName("secretary_id");
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(c => c.Code).IsUnique();

        // Председатель и секретарь не могут быть одним лицом
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_committees_chair_secretary_different",
            "\"chair_id\" IS NULL OR \"secretary_id\" IS NULL OR \"chair_id\" != \"secretary_id\""));

        builder.HasOne(c => c.Chair)
            .WithMany()
            .HasForeignKey(c => c.ChairId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.Secretary)
            .WithMany()
            .HasForeignKey(c => c.SecretaryId)
            .OnDelete(DeleteBehavior.SetNull);

        // Базовое наполнение: 10 комитетов Совета директоров
        builder.HasData(
            new Committee
            {
                Id = new Guid("10000000-0000-0000-0000-000000000001"),
                Code = "AUDIT",
                Name = "По аудиту",
                BehaviorType = BehaviorType.CONTROL,
                Description = "Контроль финансовой отчетности, оценка независимости и качества работы внешнего аудитора, взаимодействие с ревизионной комиссией и службой внутреннего аудита, мониторинг систем управления рисками и внутреннего контроля.",
                IsMandatoryForPublic = true,
                IsActive = true,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Committee
            {
                Id = new Guid("10000000-0000-0000-0000-000000000002"),
                Code = "HR_N_REM",
                Name = "По кадрам и вознаграждениям",
                BehaviorType = BehaviorType.CONTROL,
                Description = "Разработка политики вознаграждения для членов Совета директоров и исполнительных органов, определение критериев подбора кандидатов в органы управления, планирование преемственности.",
                IsMandatoryForPublic = false,
                IsActive = true,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Committee
            {
                Id = new Guid("10000000-0000-0000-0000-000000000003"),
                Code = "STRATEGY",
                Name = "По стратегии",
                BehaviorType = BehaviorType.STRATEGIC,
                Description = "Предварительное рассмотрение вопросов стратегического развития, контроль реализации долгосрочных целей, выработка рекомендаций по дивидендной политике.",
                IsMandatoryForPublic = false,
                IsActive = true,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Committee
            {
                Id = new Guid("10000000-0000-0000-0000-000000000004"),
                Code = "FINANCE",
                Name = "По финансам",
                BehaviorType = BehaviorType.CONTROL,
                Description = "Предварительное рассмотрение финансовых планов и бюджетов, мониторинг финансовых показателей, анализ инвестиционных проектов.",
                IsMandatoryForPublic = false,
                IsActive = true,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Committee
            {
                Id = new Guid("10000000-0000-0000-0000-000000000005"),
                Code = "HSE",
                Name = "По охране труда, промышленной безопасности и экологии",
                BehaviorType = BehaviorType.CONTROL,
                Description = "Контроль соблюдения требований охраны труда, промышленной безопасности и экологического законодательства.",
                IsMandatoryForPublic = false,
                IsActive = true,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Committee
            {
                Id = new Guid("10000000-0000-0000-0000-000000000006"),
                Code = "CG",
                Name = "По корпоративному управлению",
                BehaviorType = BehaviorType.CONTROL,
                Description = "Совершенствование практик корпоративного управления, контроль соблюдения этических норм, взаимодействие с акционерами.",
                IsMandatoryForPublic = false,
                IsActive = true,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Committee
            {
                Id = new Guid("10000000-0000-0000-0000-000000000007"),
                Code = "RISK",
                Name = "По рискам",
                BehaviorType = BehaviorType.CONTROL,
                Description = "Идентификация и мониторинг существенных рисков, разработка мер по их минимизации, контроль эффективности системы управления рисками.",
                IsMandatoryForPublic = false,
                IsActive = true,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Committee
            {
                Id = new Guid("10000000-0000-0000-0000-000000000008"),
                Code = "INVEST",
                Name = "По инвестициям",
                BehaviorType = BehaviorType.STRATEGIC,
                Description = "Рассмотрение и оценка инвестиционных проектов, контроль их реализации и эффективности.",
                IsMandatoryForPublic = false,
                IsActive = true,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Committee
            {
                Id = new Guid("10000000-0000-0000-0000-000000000009"),
                Code = "CSR",
                Name = "По корпоративной социальной ответственности",
                BehaviorType = BehaviorType.STRATEGIC,
                Description = "Разработка и контроль реализации политики в области КСО, устойчивого развития и взаимодействия с заинтересованными сторонами.",
                IsMandatoryForPublic = false,
                IsActive = true,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Committee
            {
                Id = new Guid("10000000-0000-0000-0000-00000000000A"),
                Code = "REI",
                Name = "По надежности, энергоэффективности и инновациям",
                BehaviorType = BehaviorType.STRATEGIC,
                Description = "Контроль надежности производственных мощностей, повышение энергоэффективности и внедрение инноваций.",
                IsMandatoryForPublic = false,
                IsActive = true,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
