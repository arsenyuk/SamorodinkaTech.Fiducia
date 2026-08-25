using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class PdnConsentConfiguration : IEntityTypeConfiguration<PdnConsent>
{
    public void Configure(EntityTypeBuilder<PdnConsent> builder)
    {
        builder.ToTable("pdn_consents");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.EcosystemParticipantId).HasColumnName("ecosystem_participant_id").IsRequired();
        builder.Property(c => c.LegalEntityId).HasColumnName("legal_entity_id").IsRequired();
        builder.Property(c => c.ConsentGiven).HasColumnName("consent_given").HasDefaultValue(false);
        builder.Property(c => c.ConsentAt).HasColumnName("consent_at");
        builder.Property(c => c.ConsentIp).HasColumnName("consent_ip").HasMaxLength(45);
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(c => c.EcosystemParticipantId);
        builder.HasIndex(c => c.LegalEntityId);
        builder.HasIndex(c => new { c.EcosystemParticipantId, c.LegalEntityId }, "ux_pdn_consents_ep_le").IsUnique();

        builder.HasOne(c => c.EcosystemParticipant)
            .WithMany(p => p.PdnConsents)
            .HasForeignKey(c => c.EcosystemParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.LegalEntity)
            .WithMany()
            .HasForeignKey(c => c.LegalEntityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}