using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class SecurityAuditLogConfiguration : IEntityTypeConfiguration<SecurityAuditLog>
{
    public void Configure(EntityTypeBuilder<SecurityAuditLog> builder)
    {
        builder.ToTable("security_audit_log");

        builder.HasKey(sal => sal.Id);
        builder.Property(sal => sal.Id).HasColumnName("id").UseIdentityAlwaysColumn();

        builder.Property(sal => sal.UserId).HasColumnName("user_id");
        builder.Property(sal => sal.UserIp).HasColumnName("user_ip").HasMaxLength(45).IsRequired();
        builder.Property(sal => sal.ActionCode).HasColumnName("action_code").HasMaxLength(100).IsRequired();
        builder.Property(sal => sal.EntityName).HasColumnName("entity_name").HasMaxLength(100);
        builder.Property(sal => sal.EntityId).HasColumnName("entity_id");
        builder.Property(sal => sal.Description).HasColumnName("description").IsRequired();
        builder.Property(sal => sal.LogTimestamp).HasColumnName("log_timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(sal => sal.UserId);
        builder.HasIndex(sal => sal.ActionCode);
        builder.HasIndex(sal => sal.LogTimestamp);
    }
}
