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

        builder.Property(u => u.LastName).HasColumnName("last_name").HasMaxLength(150).IsRequired();
        builder.Property(u => u.FirstName).HasColumnName("first_name").HasMaxLength(150).IsRequired();
        builder.Property(u => u.MiddleName).HasColumnName("middle_name").HasMaxLength(150);
        builder.Property(u => u.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
        builder.Property(u => u.Phone).HasColumnName("phone").HasMaxLength(20).IsRequired();
        builder.Property(u => u.IsExternal).HasColumnName("is_external").HasDefaultValue(false);
        builder.Property(u => u.PepAgreementSigned).HasColumnName("pep_agreement_signed").HasDefaultValue(false);
        builder.Property(u => u.PepSignedAt).HasColumnName("pep_signed_at");
        builder.Property(u => u.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(u => u.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(u => u.AccountExpiresAt).HasColumnName("account_expires_at");
        builder.Property(u => u.LdapCreatedAt).HasColumnName("ldap_created_at");

        // Онбординг внешних директоров и согласия ПДн (snake_case)
        builder.Property(u => u.InvitationToken).HasColumnName("invitation_token");
        builder.Property(u => u.InvitationExpiresAt).HasColumnName("invitation_expires_at");
        builder.Property(u => u.DeclarationCompleted).HasColumnName("declaration_completed");
        builder.Property(u => u.DeclarationData).HasColumnName("declaration_data");
        builder.Property(u => u.PdnConsentGiven).HasColumnName("pdn_consent_given");
        builder.Property(u => u.PdnConsentAt).HasColumnName("pdn_consent_at");
        builder.Property(u => u.PdnConsentIp).HasColumnName("pdn_consent_ip");

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.Phone).IsUnique();
    }
}
