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
        b.Property(x => x.FileType).HasColumnName("file_type").HasMaxLength(50);
        b.Property(x => x.DisplayName).HasColumnName("display_name").HasMaxLength(255);
        b.Property(x => x.Extension).HasColumnName("extension").HasMaxLength(20);
        b.Property(x => x.IsUploaded).HasColumnName("is_uploaded").HasDefaultValue(true);
        b.Property(x => x.UploadId).HasColumnName("upload_id").HasMaxLength(64);
        b.Property(x => x.ExpiresAt).HasColumnName("expires_at");

        b.HasIndex(x => new { x.StorageProvider, x.StorageKeyOrPath })
            .IsUnique()
            .HasDatabaseName("ux_files_provider_key");
        b.HasIndex(x => x.CreatedAt).HasDatabaseName("ix_files_created_at");
        b.HasIndex(x => x.Checksum).HasDatabaseName("ix_files_checksum");
        b.HasIndex(x => x.UploadId).HasDatabaseName("ix_files_upload_id").HasFilter("upload_id IS NOT NULL");
    }
}
