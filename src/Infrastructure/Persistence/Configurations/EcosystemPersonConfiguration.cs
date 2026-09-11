using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class EcosystemPersonConfiguration : IEntityTypeConfiguration<EcosystemPerson>
{
    public void Configure(EntityTypeBuilder<EcosystemPerson> builder)
    {
        builder.ToTable("ecosystem_persons");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(150).IsRequired();
        builder.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(150).IsRequired();
        builder.Property(x => x.MiddleName).HasColumnName("middle_name").HasMaxLength(150);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
    }
}
