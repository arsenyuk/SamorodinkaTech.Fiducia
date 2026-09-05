using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class EcosystemParticipantConfiguration : IEntityTypeConfiguration<EcosystemParticipant>
{
    public void Configure(EntityTypeBuilder<EcosystemParticipant> builder)
    {
        builder.ToTable("ecosystem_participants");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.LegalEntityId).HasColumnName("legal_entity_id").IsRequired();
        builder.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(150).IsRequired();
        builder.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(150).IsRequired();
        builder.Property(x => x.MiddleName).HasColumnName("middle_name").HasMaxLength(150);
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(255);
        builder.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(20);
        builder.Property(x => x.Login).HasColumnName("login").HasMaxLength(100).IsRequired();
        builder.Property(x => x.UserId).HasColumnName("user_id");

        // MPI: мастер-запись (источник: ЕДИН)
        builder.Property(x => x.MpiMasterId).HasColumnName("mpi_master_id");
        builder.HasIndex(x => x.MpiMasterId).HasDatabaseName("ix_ecosystem_participant_mpi_master_id");

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");

        builder.HasIndex(x => new { x.LegalEntityId, x.Login }).IsUnique().HasDatabaseName("ux_ecosystem_participant_le_login");
        builder.HasIndex(x => x.LegalEntityId).HasDatabaseName("ix_ecosystem_participant_le");

        builder.HasOne(x => x.LegalEntity)
            .WithMany()
            .HasForeignKey(x => x.LegalEntityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
