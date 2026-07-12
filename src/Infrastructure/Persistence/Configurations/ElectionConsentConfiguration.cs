using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class ElectionConsentConfiguration : IEntityTypeConfiguration<ElectionConsent>
{
    public void Configure(EntityTypeBuilder<ElectionConsent> builder)
    {
        builder.ToTable("election_consents");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ProposalId).HasColumnName("proposal_id").IsRequired();
        builder.Property(x => x.CandidateMemberId).HasColumnName("candidate_member_id").IsRequired();
        builder.Property(x => x.ConsentGiven).HasColumnName("consent_given").IsRequired();
        builder.Property(x => x.ConsentToken).HasColumnName("consent_token").HasMaxLength(64).IsRequired();
        builder.Property(x => x.SignedAt).HasColumnName("signed_at");
        builder.Property(x => x.SignedIp).HasColumnName("signed_ip").HasMaxLength(45);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(x => x.Proposal).WithMany().HasForeignKey(x => x.ProposalId);
        builder.HasOne(x => x.CandidateMember).WithMany().HasForeignKey(x => x.CandidateMemberId);

        builder.HasIndex(x => new { x.ProposalId, x.CandidateMemberId }).IsUnique();
        builder.HasIndex(x => x.ConsentToken).IsUnique();
        builder.HasIndex(x => x.ProposalId);
    }
}
