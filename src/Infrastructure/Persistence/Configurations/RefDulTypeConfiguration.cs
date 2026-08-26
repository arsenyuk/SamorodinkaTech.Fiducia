using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

/// <summary>
/// Конфигурация справочника видов документов, удостоверяющих личность.
/// </summary>
public class RefDulTypeConfiguration : IEntityTypeConfiguration<RefDulType>
{
    public void Configure(EntityTypeBuilder<RefDulType> builder)
    {
        builder.ToTable("ref_dul_type");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(10).IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(500).IsRequired();
        builder.Property(x => x.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
        builder.Property(x => x.HasSeries).HasColumnName("has_series").HasDefaultValue(true);
        builder.Property(x => x.HasDepartmentCode).HasColumnName("has_department_code").HasDefaultValue(false);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by").IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();
    }
}
