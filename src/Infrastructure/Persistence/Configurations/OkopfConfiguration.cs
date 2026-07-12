using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

/// <summary>
/// Конфигурация справочника ОКОПФ.
/// </summary>
public class OkopfConfiguration : IEntityTypeConfiguration<Okopf>
{
    public void Configure(EntityTypeBuilder<Okopf> builder)
    {
        builder.ToTable("ref_okopf");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id");
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(10)
            .IsRequired();
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(x => x.Name).HasDatabaseName("ix_ref_okopf_name");
    }
}
