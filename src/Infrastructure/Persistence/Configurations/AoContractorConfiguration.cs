using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Enums;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class AoContractorConfiguration : IEntityTypeConfiguration<AoContractor>
{
    public void Configure(EntityTypeBuilder<AoContractor> builder)
    {
        builder.ToTable("ao_contractors");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.LegalEntityId).HasColumnName("legal_entity_id").IsRequired();
        builder.Property(c => c.ContractorInn).HasColumnName("contractor_inn").HasMaxLength(12).IsRequired();
        builder.Property(c => c.ContractorName).HasColumnName("contractor_name").HasMaxLength(500).IsRequired();
        builder.Property(c => c.ContractorType)
            .HasColumnName("contractor_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.ContractNumber).HasColumnName("contract_number").HasMaxLength(100);
        builder.Property(c => c.ContractDate).HasColumnName("contract_date");
        builder.Property(c => c.ContractValidFrom).HasColumnName("contract_valid_from");
        builder.Property(c => c.ContractValidTo).HasColumnName("contract_valid_to");
        builder.Property(c => c.IsIndefinite).HasColumnName("is_indefinite").HasDefaultValue(true);

        builder.Property(c => c.ContractDocumentId).HasColumnName("contract_document_id");

        builder.Property(c => c.RegistryPreparationDays).HasColumnName("registry_preparation_days");
        builder.Property(c => c.RegistryPreparationUnit)
            .HasColumnName("registry_preparation_unit")
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(c => c.DividendRegistryPreparationDays).HasColumnName("dividend_registry_preparation_days");
        builder.Property(c => c.DividendRegistryPreparationUnit)
            .HasColumnName("dividend_registry_preparation_unit")
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(c => c.RegistryRulesUrl).HasColumnName("registry_rules_url").HasMaxLength(1000);
        builder.Property(c => c.RegistryRulesDocumentId).HasColumnName("registry_rules_document_id");

        builder.Property(c => c.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(c => c.CreatedBy).HasColumnName("created_by");

        builder.HasIndex(c => c.LegalEntityId).HasDatabaseName("ix_ao_contractors_legal_entity_id");

        builder.HasOne(c => c.LegalEntity)
            .WithMany()
            .HasForeignKey(c => c.LegalEntityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.ContractDocument)
            .WithMany()
            .HasForeignKey(c => c.ContractDocumentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.RegistryRulesDocument)
            .WithMany()
            .HasForeignKey(c => c.RegistryRulesDocumentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
