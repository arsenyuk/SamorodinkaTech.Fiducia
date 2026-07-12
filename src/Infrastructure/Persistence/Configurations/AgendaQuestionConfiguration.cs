using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Enums;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class AgendaQuestionConfiguration : IEntityTypeConfiguration<AgendaQuestion>
{
    public void Configure(EntityTypeBuilder<AgendaQuestion> builder)
    {
        builder.ToTable("agenda_questions");

        builder.HasKey(aq => aq.Id);
        builder.Property(aq => aq.Id).HasColumnName("id");

        builder.Property(aq => aq.MeetingId).HasColumnName("meeting_id").IsRequired();
        builder.Property(aq => aq.SequenceNumber).HasColumnName("sequence_number").IsRequired();
        builder.Property(aq => aq.QuestionText).HasColumnName("question_text").IsRequired();
        builder.Property(aq => aq.ProposedResolution).HasColumnName("proposed_resolution").IsRequired();
        builder.Property(aq => aq.Status).HasColumnName("status").HasMaxLength(50)
            .HasConversion<string>().HasDefaultValue(QuestionStatus.PENDING);

        builder.HasIndex(aq => aq.MeetingId);
        builder.HasIndex(aq => aq.Status);

        builder.HasOne(aq => aq.Meeting)
            .WithMany(m => m.AgendaQuestions)
            .HasForeignKey(aq => aq.MeetingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
