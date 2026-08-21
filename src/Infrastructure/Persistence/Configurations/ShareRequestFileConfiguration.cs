using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class ShareRequestFileConfiguration : IEntityTypeConfiguration<ShareRequestFile>
{
    public void Configure(EntityTypeBuilder<ShareRequestFile> b)
    {
        b.ToTable("share_request_files");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.ShareRequestId).HasColumnName("share_request_id").IsRequired();
        b.Property(x => x.FileId).HasColumnName("file_id").IsRequired();

        b.HasIndex(x => x.ShareRequestId).HasDatabaseName("ix_srf_request");
        b.HasIndex(x => x.FileId).HasDatabaseName("ix_srf_file");

        b.HasOne(x => x.ShareRequest)
            .WithMany()
            .HasForeignKey(x => x.ShareRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.File)
            .WithMany()
            .HasForeignKey(x => x.FileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
