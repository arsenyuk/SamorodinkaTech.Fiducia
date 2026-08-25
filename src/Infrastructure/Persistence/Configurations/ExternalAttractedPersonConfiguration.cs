using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class ExternalAttractedPersonConfiguration : IEntityTypeConfiguration<ExternalAttractedPerson>
{
    public void Configure(EntityTypeBuilder<ExternalAttractedPerson> builder)
    {
        builder.ToTable("external_attracted_persons");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.EcosystemParticipantId).HasColumnName("ecosystem_participant_id").IsRequired();
        builder.Property(e => e.LegalEntityId).HasColumnName("legal_entity_id").IsRequired();
        builder.Property(e => e.Position).HasColumnName("position").HasMaxLength(200).IsRequired();
        builder.Property(e => e.StartedAt).HasColumnName("started_at");
        builder.Property(e => e.EndedAt).HasColumnName("ended_at");
        builder.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.CreatedBy).HasColumnName("created_by").IsRequired();

        builder.HasIndex(e => e.EcosystemParticipantId);
        builder.HasIndex(e => e.LegalEntityId);
        builder.HasIndex(e => new { e.EcosystemParticipantId, e.LegalEntityId });

        builder.HasOne(e => e.EcosystemParticipant)
            .WithMany()
            .HasForeignKey(e => e.EcosystemParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.LegalEntity)
            .WithMany()
            .HasForeignKey(e => e.LegalEntityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
