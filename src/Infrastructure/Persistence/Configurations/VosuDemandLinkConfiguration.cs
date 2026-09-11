using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class VosuDemandLinkConfiguration : IEntityTypeConfiguration<VosuDemandLink>
{
    public void Configure(EntityTypeBuilder<VosuDemandLink> builder)
    {
        builder.ToTable("vosu_demand_links");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.OrgIntentId).HasColumnName("org_intent_id").IsRequired();
        builder.Property(x => x.ShareRequestId).HasColumnName("share_request_id").IsRequired();
        builder.Property(x => x.IsInitiating).HasColumnName("is_initiating").HasDefaultValue(false);
        builder.Property(x => x.CreatedBy).HasColumnName("created_by").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(x => new { x.OrgIntentId, x.ShareRequestId }).IsUnique();

        builder.HasOne(x => x.OrgIntent)
            .WithMany()
            .HasForeignKey(x => x.OrgIntentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ShareRequest)
            .WithMany()
            .HasForeignKey(x => x.ShareRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
