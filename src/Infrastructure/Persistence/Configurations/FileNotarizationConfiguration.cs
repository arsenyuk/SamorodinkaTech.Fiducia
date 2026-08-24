using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class FileNotarizationConfiguration : IEntityTypeConfiguration<FileNotarization>
{
    public void Configure(EntityTypeBuilder<FileNotarization> b)
    {
        b.ToTable("file_notarization");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.FileId).HasColumnName("file_id").IsRequired();
        b.Property(x => x.RawUrl).HasColumnName("raw_url").HasMaxLength(2048);
        b.Property(x => x.RegistryNumber).HasColumnName("registry_number").HasMaxLength(100);
        b.Property(x => x.NotaryFullName).HasColumnName("notary_full_name").HasMaxLength(300);
        b.Property(x => x.NotarizationDate).HasColumnName("notarization_date");
        b.Property(x => x.DocumentType).HasColumnName("document_type").HasMaxLength(200);
        b.Property(x => x.ApplicantName).HasColumnName("applicant_name").HasMaxLength(300);
        b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        b.HasIndex(x => x.FileId).IsUnique().HasDatabaseName("ix_file_notarization_file_id");
        b.HasIndex(x => x.RegistryNumber).HasDatabaseName("ix_file_notarization_registry_number");

        b.HasOne(x => x.File)
            .WithMany()
            .HasForeignKey(x => x.FileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
