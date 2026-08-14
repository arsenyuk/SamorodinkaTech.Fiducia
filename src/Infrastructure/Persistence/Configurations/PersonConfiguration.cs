using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("persons");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");

        builder.Property(p => p.LastName).HasColumnName("last_name").HasMaxLength(150).IsRequired();
        builder.Property(p => p.FirstName).HasColumnName("first_name").HasMaxLength(150).IsRequired();
        builder.Property(p => p.MiddleName).HasColumnName("middle_name").HasMaxLength(150);
        builder.Property(p => p.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
        builder.Property(p => p.Phone).HasColumnName("phone").HasMaxLength(20);
        builder.Property(p => p.Inn).HasColumnName("inn").HasMaxLength(12);
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(p => p.CreatedBy).HasColumnName("created_by").IsRequired();

        builder.HasIndex(p => p.Email).IsUnique();
        builder.HasIndex(p => p.Inn);

        builder.HasOne(p => p.CreatedByUser)
            .WithMany()
            .HasForeignKey(p => p.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}