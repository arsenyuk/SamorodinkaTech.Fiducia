using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class CommitteeTaskFileConfiguration : IEntityTypeConfiguration<CommitteeTaskFile>
{
    public void Configure(EntityTypeBuilder<CommitteeTaskFile> b)
    {
        b.ToTable("committee_task_files");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.CommitteeTaskId).HasColumnName("committee_task_id").IsRequired();
        b.Property(x => x.FileId).HasColumnName("file_id").IsRequired();

        b.HasIndex(x => x.CommitteeTaskId).HasDatabaseName("ix_ctf_committee_task_id");
        b.HasIndex(x => x.FileId).HasDatabaseName("ix_ctf_file_id");

        b.HasOne(x => x.CommitteeTask)
            .WithMany()
            .HasForeignKey(x => x.CommitteeTaskId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.File)
            .WithMany()
            .HasForeignKey(x => x.FileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
