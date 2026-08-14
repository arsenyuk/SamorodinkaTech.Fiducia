using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class PepAgreementConfiguration : IEntityTypeConfiguration<PepAgreement>
{
    public void Configure(EntityTypeBuilder<PepAgreement> builder)
    {
        builder.ToTable("pep_agreements");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");

        builder.Property(a => a.PersonId).HasColumnName("person_id").IsRequired();
        builder.Property(a => a.AgreementSigned).HasColumnName("agreement_signed").HasDefaultValue(false);
        builder.Property(a => a.SignedAt).HasColumnName("signed_at");
        builder.Property(a => a.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(a => a.PersonId);

        builder.HasOne(a => a.Person)
            .WithMany(p => p.PepAgreements)
            .HasForeignKey(a => a.PersonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}