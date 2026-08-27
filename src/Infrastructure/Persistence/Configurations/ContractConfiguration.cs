using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.ToTable("contracts");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.LegalEntityId).HasColumnName("legal_entity_id").IsRequired();
        builder.Property(c => c.ContractType).HasColumnName("contract_type").HasMaxLength(30).IsRequired();

        // Данные контрагента
        builder.Property(c => c.CounterpartyName).HasColumnName("counterparty_name").HasMaxLength(500).IsRequired();
        builder.Property(c => c.CounterpartyInn).HasColumnName("counterparty_inn").HasMaxLength(12).IsRequired();

        // Реквизиты договора
        builder.Property(c => c.ContractNumber).HasColumnName("contract_number").HasMaxLength(100);
        builder.Property(c => c.ContractDate).HasColumnName("contract_date");
        builder.Property(c => c.ContractValidFrom).HasColumnName("contract_valid_from");
        builder.Property(c => c.ContractValidTo).HasColumnName("contract_valid_to");
        builder.Property(c => c.IsIndefinite).HasColumnName("is_indefinite").HasDefaultValue(true);
        builder.Property(c => c.ContractDocumentId).HasColumnName("contract_document_id");

        // Сроки подготовки реестров (REGISTRAR)
        builder.Property(c => c.RegistryPreparationDays).HasColumnName("registry_preparation_days");
        builder.Property(c => c.RegistryPreparationUnitId).HasColumnName("registry_preparation_unit");
        builder.Property(c => c.DividendRegistryPreparationDays).HasColumnName("dividend_registry_preparation_days");
        builder.Property(c => c.DividendRegistryPreparationUnitId).HasColumnName("dividend_registry_preparation_unit");
        builder.Property(c => c.RegistryRulesUrl).HasColumnName("registry_rules_url").HasMaxLength(1000);
        builder.Property(c => c.RegistryRulesDocumentId).HasColumnName("registry_rules_document_id");

        // Управляющий ИП (MANAGEMENT_IP)
        builder.Property(c => c.ManagerOgrnip).HasColumnName("manager_ogrnip").HasMaxLength(15);

        // Управляющий ЮЛ (MANAGEMENT_UL)
        builder.Property(c => c.ManagerLegalEntityId).HasColumnName("manager_legal_entity_id");

        // Статус
        builder.Property(c => c.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(c => c.CreatedBy).HasColumnName("created_by");

        // Индексы
        builder.HasIndex(c => c.LegalEntityId).HasDatabaseName("ix_contracts_le_id");

        // FK: LegalEntity → Restrict
        builder.HasOne(c => c.LegalEntity)
            .WithMany()
            .HasForeignKey(c => c.LegalEntityId)
            .OnDelete(DeleteBehavior.Restrict);

        // FK: ContractDocument → SetNull
        builder.HasOne(c => c.ContractDocument)
            .WithMany()
            .HasForeignKey(c => c.ContractDocumentId)
            .OnDelete(DeleteBehavior.SetNull);

        // FK: RegistryPreparationUnit → Restrict
        builder.HasOne(c => c.RegistryPreparationUnit)
            .WithMany()
            .HasForeignKey(c => c.RegistryPreparationUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        // FK: DividendRegistryPreparationUnit → Restrict
        builder.HasOne(c => c.DividendRegistryPreparationUnit)
            .WithMany()
            .HasForeignKey(c => c.DividendRegistryPreparationUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        // FK: RegistryRulesDocument → SetNull
        builder.HasOne(c => c.RegistryRulesDocument)
            .WithMany()
            .HasForeignKey(c => c.RegistryRulesDocumentId)
            .OnDelete(DeleteBehavior.SetNull);

        // FK: ManagerLegalEntity → SetNull
        builder.HasOne(c => c.ManagerLegalEntity)
            .WithMany()
            .HasForeignKey(c => c.ManagerLegalEntityId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
