using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class TrueConfTestQuestionConfiguration : IEntityTypeConfiguration<TrueConfTestQuestion>
{
    public void Configure(EntityTypeBuilder<TrueConfTestQuestion> builder)
    {
        builder.ToTable("trueconf_test_question");

        builder.HasKey(q => q.Id);
        builder.Property(q => q.Id).HasColumnName("id");

        builder.Property(q => q.MeetingId).HasColumnName("meeting_id").IsRequired();
        builder.Property(q => q.SequenceNumber).HasColumnName("sequence_number").IsRequired();
        builder.Property(q => q.QuestionText).HasColumnName("question_text").IsRequired();
        builder.Property(q => q.ProposedResolution).HasColumnName("proposed_resolution").HasDefaultValue("");
        builder.Property(q => q.TrueConfPollId).HasColumnName("trueconf_poll_id").HasMaxLength(100);
        builder.Property(q => q.PollState).HasColumnName("poll_state").HasMaxLength(20);
        builder.Property(q => q.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("PENDING");
        builder.Property(q => q.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(q => q.MeetingId);
        builder.HasIndex(q => q.TrueConfPollId);

        builder.HasOne(q => q.Meeting)
            .WithMany(m => m.Questions)
            .HasForeignKey(q => q.MeetingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
