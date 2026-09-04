using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id");

        builder.Property(u => u.Login).HasColumnName("login").HasMaxLength(100).IsRequired();
        builder.Property(u => u.LastName).HasColumnName("last_name").HasMaxLength(150).IsRequired();
        builder.Property(u => u.FirstName).HasColumnName("first_name").HasMaxLength(150).IsRequired();
        builder.Property(u => u.MiddleName).HasColumnName("middle_name").HasMaxLength(150);
        builder.Property(u => u.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
        builder.Property(u => u.Phone).HasColumnName("phone").HasMaxLength(20).IsRequired();
        builder.Property(u => u.IsExternal).HasColumnName("is_external").HasDefaultValue(false);
        builder.Property(u => u.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(u => u.CreatedBy).HasColumnName("created_by").IsRequired();
        builder.Property(u => u.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(u => u.AccountExpiresAt).HasColumnName("account_expires_at");
        builder.Property(u => u.LdapCreatedAt).HasColumnName("ldap_created_at");
        builder.Property(u => u.IsSystem).HasColumnName("is_system").HasDefaultValue(false);

        // MPI: мастер-запись (источник: LDAP)
        builder.Property(u => u.MpiMasterId).HasColumnName("mpi_master_id");
        builder.HasIndex(u => u.MpiMasterId);

        // Онбординг внешних директоров
        builder.Property(u => u.InvitationToken).HasColumnName("invitation_token");
        builder.Property(u => u.InvitationExpiresAt).HasColumnName("invitation_expires_at");

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.Login).IsUnique();
        builder.HasIndex(u => u.Phone).IsUnique();
    }
}
