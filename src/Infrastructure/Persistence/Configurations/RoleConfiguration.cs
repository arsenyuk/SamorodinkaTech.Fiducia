using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("ref_roles");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");

        builder.Property(r => r.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(r => r.Name).HasColumnName("name").HasMaxLength(100).IsRequired();

        builder.HasIndex(r => r.Code).IsUnique();
    }
}
