using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class ShareRequestItemConfiguration : IEntityTypeConfiguration<ShareRequestItem>
{
    public void Configure(EntityTypeBuilder<ShareRequestItem> b)
    {
        b.ToTable("share_request_items");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.ShareRequestId).HasColumnName("share_request_id").IsRequired();
        b.Property(x => x.SequenceNumber).HasColumnName("sequence_number").IsRequired();
        b.Property(x => x.Title).HasColumnName("title").IsRequired().HasMaxLength(500);
        b.Property(x => x.Description).HasColumnName("description");
        b.Property(x => x.Status).HasColumnName("status").IsRequired().HasMaxLength(20).HasDefaultValue("pending");
        b.Property(x => x.RejectionReason).HasColumnName("rejection_reason");
        b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        b.HasIndex(x => x.ShareRequestId).HasDatabaseName("ix_sri_request");
        b.HasIndex(x => new { x.ShareRequestId, x.SequenceNumber }).HasDatabaseName("ix_sri_request_seq");

        b.HasOne(x => x.ShareRequest)
            .WithMany(r => r.Items)
            .HasForeignKey(x => x.ShareRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
