using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class CommitteeMemberConfiguration : IEntityTypeConfiguration<CommitteeMember>
{
    public void Configure(EntityTypeBuilder<CommitteeMember> builder)
    {
        builder.ToTable("committee_members");

        builder.HasKey(cm => cm.Id);
        builder.Property(cm => cm.Id).HasColumnName("id").ValueGeneratedOnAdd();

        builder.Property(cm => cm.CommitteeId).HasColumnName("committee_id");
        builder.Property(cm => cm.UserId).HasColumnName("user_id");

        builder.HasIndex(cm => new { cm.CommitteeId, cm.UserId }).IsUnique();

        builder.HasOne(cm => cm.Committee)
            .WithMany(c => c.Members)
            .HasForeignKey(cm => cm.CommitteeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cm => cm.User)
            .WithMany(u => u.CommitteeMembers)
            .HasForeignKey(cm => cm.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
