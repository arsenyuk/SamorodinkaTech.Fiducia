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

        builder.Property(c => c.PersonId).HasColumnName("person_id").IsRequired();
        builder.Property(c => c.ConsentGiven).HasColumnName("consent_given").HasDefaultValue(false);
        builder.Property(c => c.ConsentAt).HasColumnName("consent_at");
        builder.Property(c => c.ConsentIp).HasColumnName("consent_ip").HasMaxLength(45);
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(c => c.PersonId);

        builder.HasOne(c => c.Person)
            .WithMany(p => p.PdnConsents)
            .HasForeignKey(c => c.PersonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}