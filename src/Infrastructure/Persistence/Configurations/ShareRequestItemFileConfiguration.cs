using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class ShareRequestItemFileConfiguration : IEntityTypeConfiguration<ShareRequestItemFile>
{
    public void Configure(EntityTypeBuilder<ShareRequestItemFile> b)
    {
        b.ToTable("share_request_item_files");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.ShareRequestItemId).HasColumnName("share_request_item_id").IsRequired();
        b.Property(x => x.FileId).HasColumnName("file_id").IsRequired();
        b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        b.HasIndex(x => x.ShareRequestItemId).HasDatabaseName("ix_srif_item");
        b.HasIndex(x => x.FileId).HasDatabaseName("ix_srif_file");
        b.HasIndex(x => new { x.ShareRequestItemId, x.FileId }).IsUnique().HasDatabaseName("ix_srif_item_file");

        b.HasOne(x => x.ShareRequestItem)
            .WithMany(i => i.Files)
            .HasForeignKey(x => x.ShareRequestItemId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.File)
            .WithMany()
            .HasForeignKey(x => x.FileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
