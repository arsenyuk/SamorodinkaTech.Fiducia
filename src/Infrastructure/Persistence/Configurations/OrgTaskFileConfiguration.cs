using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class OrgTaskFileConfiguration : IEntityTypeConfiguration<OrgTaskFile>
{
    public void Configure(EntityTypeBuilder<OrgTaskFile> b)
    {
        b.ToTable("org_task_files");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.OrgTaskId).HasColumnName("org_task_id").IsRequired();
        b.Property(x => x.FileId).HasColumnName("file_id").IsRequired();

        b.HasIndex(x => x.OrgTaskId).HasDatabaseName("ix_otf_org_task_id");
        b.HasIndex(x => x.FileId).HasDatabaseName("ix_otf_file_id");

        b.HasOne(x => x.OrgTask)
            .WithMany()
            .HasForeignKey(x => x.OrgTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.File)
            .WithMany()
            .HasForeignKey(x => x.FileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
