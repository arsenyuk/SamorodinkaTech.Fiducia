using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class RefRequestTypeConfiguration : IEntityTypeConfiguration<RefRequestType>
{
    public void Configure(EntityTypeBuilder<RefRequestType> builder)
    {
        builder.ToTable("ref_request_type");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.Code).HasColumnName("code").IsRequired().HasMaxLength(50);
        builder.Property(r => r.Name).HasColumnName("name").IsRequired().HasMaxLength(300);
        builder.Property(r => r.IsForLlc).HasColumnName("is_for_llc").IsRequired().HasDefaultValue(false);
        builder.Property(r => r.IsForNjsc).HasColumnName("is_for_njsc").IsRequired().HasDefaultValue(false);
        builder.Property(r => r.IsForPjsc).HasColumnName("is_for_pjsc").IsRequired().HasDefaultValue(false);
        builder.Property(r => r.RequiresFile).HasColumnName("requires_file").IsRequired().HasDefaultValue(false);
        builder.Property(r => r.ConsideredByOsu).HasColumnName("considered_by_osu").IsRequired().HasDefaultValue(false);
        builder.Property(r => r.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(r => r.Code).IsUnique();
    }
}
