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
        builder.Property(r => r.RequestTypeId).HasColumnName("request_type_id").IsRequired();
        builder.Property(r => r.Status).HasColumnName("status").IsRequired().HasMaxLength(20).HasDefaultValue("pending");
        builder.Property(r => r.Payload).HasColumnName("payload");
        builder.Property(r => r.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(r => r.CompletedAt).HasColumnName("completed_at");
        builder.Property(r => r.CreatedBy).HasColumnName("created_by");
        builder.Property(r => r.RevokedAt).HasColumnName("revoked_at");
        builder.Property(r => r.RevokedByNotarized).HasColumnName("revoked_by_notarized").HasDefaultValue(false);
        builder.Property(r => r.VisibleToAll).HasColumnName("visible_to_all").HasDefaultValue(false);
        builder.Property(r => r.NotarizationId).HasColumnName("notarization_id");

        builder.HasIndex(r => r.LegalEntityId);
        builder.HasIndex(r => r.ParticipantId);
        builder.HasIndex(r => r.RequestTypeId);
        builder.HasIndex(r => r.Status);

        builder.HasOne(r => r.LegalEntity)
            .WithMany()
            .HasForeignKey(r => r.LegalEntityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Participant)
            .WithMany()
            .HasForeignKey(r => r.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.RequestType)
            .WithMany()
            .HasForeignKey(r => r.RequestTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Creator)
            .WithMany()
            .HasForeignKey(r => r.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.Notarization)
            .WithMany()
            .HasForeignKey(r => r.NotarizationId)
            .OnDelete(DeleteBehavior.SetNull);

        // Коллективные требования
        builder.Property(r => r.IsCollective).HasColumnName("is_collective").HasDefaultValue(false);
        builder.Property(r => r.ThresholdPercent).HasColumnName("threshold_percent");
        builder.Property(r => r.TotalSupportPercent).HasColumnName("total_support_percent").HasDefaultValue(0m);
        builder.Property(r => r.SupporterCount).HasColumnName("supporter_count").HasDefaultValue(0);
        builder.Property(r => r.CollectiveStatus).HasColumnName("collective_status").HasMaxLength(20);
        builder.Property(r => r.SubmittedToCeoAt).HasColumnName("submitted_to_ceo_at");
        builder.Property(r => r.CeoDecisionAt).HasColumnName("ceo_decision_at");
        builder.Property(r => r.CeoComment).HasColumnName("ceo_comment");
        builder.Property(r => r.ReviewLocation).HasColumnName("review_location");
        builder.Property(r => r.DecidedByUserId).HasColumnName("decided_by_user_id");

        // Решение по требованию
        builder.Property(r => r.DecisionStatus).HasColumnName("decision_status").HasMaxLength(20);
        builder.Property(r => r.DecisionComment).HasColumnName("decision_comment");
        builder.Property(r => r.DecidedAt).HasColumnName("decided_at");

        builder.HasOne(r => r.DecidedByUser)
            .WithMany()
            .HasForeignKey(r => r.DecidedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Орг-план ВОСУ
        builder.Property(r => r.OrgIntentId).HasColumnName("org_intent_id");
        builder.HasOne(r => r.OrgIntent)
            .WithMany()
            .HasForeignKey(r => r.OrgIntentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
