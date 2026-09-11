using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class VosuNotificationConfiguration : IEntityTypeConfiguration<VosuNotification>
{
    public void Configure(EntityTypeBuilder<VosuNotification> builder)
    {
        builder.ToTable("vosu_notifications");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.OrgIntentId).HasColumnName("org_intent_id").IsRequired();
        builder.Property(x => x.BoardParticipantId).HasColumnName("board_participant_id").IsRequired();
        builder.Property(x => x.FileId).HasColumnName("file_id").IsRequired();
        builder.Property(x => x.CreatedBy).HasColumnName("created_by").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(x => x.OrgIntentId);

        builder.HasOne(x => x.OrgIntent)
            .WithMany()
            .HasForeignKey(x => x.OrgIntentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.BoardParticipant)
            .WithMany()
            .HasForeignKey(x => x.BoardParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.File)
            .WithMany()
            .HasForeignKey(x => x.FileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
