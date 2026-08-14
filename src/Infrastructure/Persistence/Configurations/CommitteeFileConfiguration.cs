using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class CommitteeFileConfiguration : IEntityTypeConfiguration<CommitteeFile>
{
    public void Configure(EntityTypeBuilder<CommitteeFile> b)
    {
        b.ToTable("committee_files");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.CommitteeId).HasColumnName("committee_id").IsRequired();
        b.Property(x => x.FileId).HasColumnName("file_id").IsRequired();

        b.HasIndex(x => x.CommitteeId).HasDatabaseName("ix_cf_committee_id");
        b.HasIndex(x => x.FileId).HasDatabaseName("ix_cf_file_id");

        b.HasOne(x => x.Committee)
            .WithMany()
            .HasForeignKey(x => x.CommitteeId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.File)
            .WithMany()
            .HasForeignKey(x => x.FileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
