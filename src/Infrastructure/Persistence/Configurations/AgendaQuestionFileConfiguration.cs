using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class AgendaQuestionFileConfiguration : IEntityTypeConfiguration<AgendaQuestionFile>
{
    public void Configure(EntityTypeBuilder<AgendaQuestionFile> b)
    {
        b.ToTable("agenda_question_files");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.AgendaQuestionId).HasColumnName("agenda_question_id").IsRequired();
        b.Property(x => x.FileId).HasColumnName("file_id").IsRequired();

        b.HasIndex(x => x.AgendaQuestionId).HasDatabaseName("ix_aqf_agenda_question_id");
        b.HasIndex(x => x.FileId).HasDatabaseName("ix_aqf_file_id");

        b.HasOne(x => x.AgendaQuestion)
            .WithMany()
            .HasForeignKey(x => x.AgendaQuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.File)
            .WithMany()
            .HasForeignKey(x => x.FileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
