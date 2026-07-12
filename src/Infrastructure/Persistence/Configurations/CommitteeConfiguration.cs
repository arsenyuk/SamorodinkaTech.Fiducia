using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

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
    }
}
