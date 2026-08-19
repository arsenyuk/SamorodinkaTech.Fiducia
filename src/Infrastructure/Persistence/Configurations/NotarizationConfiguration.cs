using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class NotarizationConfiguration : IEntityTypeConfiguration<Notarization>
{
    public void Configure(EntityTypeBuilder<Notarization> builder)
    {
        builder.ToTable("notarization");

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).HasColumnName("id");

        builder.Property(n => n.LegalEntityId).HasColumnName("legal_entity_id").IsRequired();
        builder.Property(n => n.DocumentType).HasColumnName("document_type").IsRequired().HasMaxLength(50);
        builder.Property(n => n.RelatedEntityId).HasColumnName("related_entity_id");
        builder.Property(n => n.RelatedEntityType).HasColumnName("related_entity_type").HasMaxLength(50);
        builder.Property(n => n.DocumentFileId).HasColumnName("document_file_id").IsRequired();
        builder.Property(n => n.NotaryFullName).HasColumnName("notary_full_name").IsRequired().HasMaxLength(300);
        builder.Property(n => n.NotaryLicenseNumber).HasColumnName("notary_license_number").HasMaxLength(100);
        builder.Property(n => n.RegistryNumber).HasColumnName("registry_number").HasMaxLength(100);
        builder.Property(n => n.NotarizationDate).HasColumnName("notarization_date");
        builder.Property(n => n.ValidFrom).HasColumnName("valid_from");
        builder.Property(n => n.ValidUntil).HasColumnName("valid_until");
        builder.Property(n => n.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(n => n.CreatedBy).HasColumnName("created_by");

        builder.HasIndex(n => n.LegalEntityId);
        builder.HasIndex(n => n.DocumentType);
        builder.HasIndex(n => new { n.RelatedEntityType, n.RelatedEntityId });

        builder.HasOne(n => n.DocumentFile)
            .WithMany()
            .HasForeignKey(n => n.DocumentFileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(n => n.LegalEntity)
            .WithMany()
            .HasForeignKey(n => n.LegalEntityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
