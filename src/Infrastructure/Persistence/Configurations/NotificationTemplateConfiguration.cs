using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.ToTable("notification_template");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");

        builder.Property(t => t.NotificationTypeCode).HasColumnName("notification_type_code").HasMaxLength(50).IsRequired();
        builder.Property(t => t.TitleTemplate).HasColumnName("title_template").HasMaxLength(500).IsRequired();
        builder.Property(t => t.BodyTemplate).HasColumnName("body_template").IsRequired();
        builder.Property(t => t.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(t => t.IsEnabled).HasColumnName("is_enabled").HasDefaultValue(true);
        builder.Property(t => t.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(t => t.NotificationTypeCode).IsUnique();
    }
}
