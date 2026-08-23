using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class LlcManagementContractConfiguration : IEntityTypeConfiguration<LlcManagementContract>
{
    public void Configure(EntityTypeBuilder<LlcManagementContract> builder)
    {
        builder.ToTable("llc_management_contracts");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.LegalEntityId).HasColumnName("legal_entity_id").IsRequired();
        builder.Property(c => c.ManagerFullName).HasColumnName("manager_full_name").HasMaxLength(500).IsRequired();
        builder.Property(c => c.ManagerInn).HasColumnName("manager_inn").HasMaxLength(12).IsRequired();
        builder.Property(c => c.ManagerOgrnip).HasColumnName("manager_ogrnip").HasMaxLength(15);

        builder.Property(c => c.ContractNumber).HasColumnName("contract_number").HasMaxLength(100);
        builder.Property(c => c.ContractDate).HasColumnName("contract_date");
        builder.Property(c => c.ContractValidFrom).HasColumnName("contract_valid_from").IsRequired();
        builder.Property(c => c.ContractValidTo).HasColumnName("contract_valid_to");
        builder.Property(c => c.IsIndefinite).HasColumnName("is_indefinite").HasDefaultValue(true);

        builder.Property(c => c.ContractDocumentId).HasColumnName("contract_document_id");

        builder.Property(c => c.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(c => c.CreatedBy).HasColumnName("created_by");

        builder.HasIndex(c => c.LegalEntityId).HasDatabaseName("ix_llc_mgmt_contracts_le_id");

        builder.HasOne(c => c.LegalEntity)
            .WithMany()
            .HasForeignKey(c => c.LegalEntityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.ContractDocument)
            .WithMany()
            .HasForeignKey(c => c.ContractDocumentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
