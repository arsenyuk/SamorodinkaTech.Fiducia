using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).HasColumnName("id").UseIdentityAlwaysColumn();

        builder.Property(n => n.UserId).HasColumnName("user_id");
        builder.Property(n => n.CommitteeId).HasColumnName("committee_id");
        builder.Property(n => n.MeetingId).HasColumnName("meeting_id");
        builder.Property(n => n.NotificationType).HasColumnName("notification_type").HasMaxLength(50).IsRequired();
        builder.Property(n => n.Title).HasColumnName("title").HasMaxLength(500).IsRequired();
        builder.Property(n => n.Body).HasColumnName("body").IsRequired();
        builder.Property(n => n.IsRead).HasColumnName("is_read").HasDefaultValue(false);
        builder.Property(n => n.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(n => n.User).WithMany().HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(n => n.Committee).WithMany().HasForeignKey(n => n.CommitteeId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(n => n.Meeting).WithMany().HasForeignKey(n => n.MeetingId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => n.CommitteeId);
        builder.HasIndex(n => n.MeetingId);
        builder.HasIndex(n => n.CreatedAt);
    }
}
