using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class MeetingFileConfiguration : IEntityTypeConfiguration<MeetingFile>
{
    public void Configure(EntityTypeBuilder<MeetingFile> b)
    {
        b.ToTable("meeting_files");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.MeetingId).HasColumnName("meeting_id").IsRequired();
        b.Property(x => x.FileId).HasColumnName("file_id").IsRequired();

        b.HasIndex(x => x.MeetingId).HasDatabaseName("ix_meeting_files_meeting_id");
        b.HasIndex(x => x.FileId).HasDatabaseName("ix_meeting_files_file_id");

        b.HasOne(x => x.Meeting)
            .WithMany()
            .HasForeignKey(x => x.MeetingId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.File)
            .WithMany()
            .HasForeignKey(x => x.FileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
