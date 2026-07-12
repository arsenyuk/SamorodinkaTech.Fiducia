using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class FileEntryConfiguration : IEntityTypeConfiguration<FileEntry>
{
    public void Configure(EntityTypeBuilder<FileEntry> b)
    {
        b.ToTable("files");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.OriginalName).HasColumnName("original_name").HasMaxLength(255).IsRequired();
        b.Property(x => x.ContentType).HasColumnName("content_type").HasMaxLength(255);
        b.Property(x => x.SizeBytes).HasColumnName("size_bytes");
        b.Property(x => x.StorageProvider).HasColumnName("storage_provider").HasMaxLength(10).IsRequired();
        b.Property(x => x.StorageKeyOrPath).HasColumnName("storage_key_or_path").HasMaxLength(1024).IsRequired();
        b.Property(x => x.Checksum).HasColumnName("checksum").HasMaxLength(64);
        b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        b.Property(x => x.CreatedBy).HasColumnName("created_by");

        b.HasIndex(x => new { x.StorageProvider, x.StorageKeyOrPath })
            .IsUnique()
            .HasDatabaseName("ux_files_provider_key");
        b.HasIndex(x => x.CreatedAt).HasDatabaseName("ix_files_created_at");
        b.HasIndex(x => x.Checksum).HasDatabaseName("ix_files_checksum");
    }
}
