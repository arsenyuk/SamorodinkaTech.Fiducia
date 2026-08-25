using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class IndependenceDeclarationConfiguration : IEntityTypeConfiguration<IndependenceDeclaration>
{
    public void Configure(EntityTypeBuilder<IndependenceDeclaration> builder)
    {
        builder.ToTable("independence_declarations");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");

        builder.Property(d => d.EcosystemParticipantId).HasColumnName("ecosystem_participant_id").IsRequired();
        builder.Property(d => d.HiddenShares).HasColumnName("hidden_shares");
        builder.Property(d => d.FamilyConnections).HasColumnName("family_connections");
        builder.Property(d => d.OtherBoards).HasColumnName("other_boards");
        builder.Property(d => d.NoCriminalRecord).HasColumnName("no_criminal_record").HasDefaultValue(false);
        builder.Property(d => d.NoBankruptcy).HasColumnName("no_bankruptcy").HasDefaultValue(false);
        builder.Property(d => d.Completed).HasColumnName("completed").HasDefaultValue(false);
        builder.Property(d => d.CompletedAt).HasColumnName("completed_at");
        builder.Property(d => d.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(d => d.EcosystemParticipantId);

        builder.HasOne(d => d.EcosystemParticipant)
            .WithMany(p => p.IndependenceDeclarations)
            .HasForeignKey(d => d.EcosystemParticipantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
