using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class ShareRequestConfiguration : IEntityTypeConfiguration<ShareRequest>
{
    public void Configure(EntityTypeBuilder<ShareRequest> builder)
    {
        builder.ToTable("share_request");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");

        builder.Property(r => r.LegalEntityId).HasColumnName("legal_entity_id").IsRequired();
        builder.Property(r => r.ParticipantId).HasColumnName("participant_id").IsRequired();
        builder.Property(r => r.RequestType).HasColumnName("request_type").IsRequired().HasMaxLength(50);
        builder.Property(r => r.Status).HasColumnName("status").IsRequired().HasMaxLength(20).HasDefaultValue("pending");
        builder.Property(r => r.Payload).HasColumnName("payload");
        builder.Property(r => r.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(r => r.CompletedAt).HasColumnName("completed_at");
        builder.Property(r => r.CreatedBy).HasColumnName("created_by");

        builder.HasIndex(r => r.LegalEntityId);
        builder.HasIndex(r => r.ParticipantId);
        builder.HasIndex(r => r.RequestType);
        builder.HasIndex(r => r.Status);

        builder.HasOne(r => r.LegalEntity)
            .WithMany()
            .HasForeignKey(r => r.LegalEntityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Participant)
            .WithMany()
            .HasForeignKey(r => r.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Creator)
            .WithMany()
            .HasForeignKey(r => r.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
