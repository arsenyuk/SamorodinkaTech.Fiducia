using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class RefNotificationTypeConfiguration : IEntityTypeConfiguration<RefNotificationType>
{
    public void Configure(EntityTypeBuilder<RefNotificationType> builder)
    {
        builder.ToTable("ref_notification_type");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");

        builder.Property(r => r.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(r => r.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(r => r.Category).HasColumnName("category").HasMaxLength(50);
        builder.Property(r => r.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(r => r.CreatedBy).HasColumnName("created_by").IsRequired();

        builder.HasIndex(r => r.Code).IsUnique();
    }
}
