using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class TrueConfTestMeetingConfiguration : IEntityTypeConfiguration<TrueConfTestMeeting>
{
    public void Configure(EntityTypeBuilder<TrueConfTestMeeting> builder)
    {
        builder.ToTable("trueconf_test_meeting");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");

        builder.Property(m => m.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(m => m.Description).HasColumnName("description");
        builder.Property(m => m.TrueConfConferenceId).HasColumnName("trueconf_conference_id").HasMaxLength(100);
        builder.Property(m => m.TrueConfJoinLink).HasColumnName("trueconf_join_link");
        builder.Property(m => m.ConferenceState).HasColumnName("conference_state").HasMaxLength(50);
        builder.Property(m => m.StartedAt).HasColumnName("started_at");
        builder.Property(m => m.EndedAt).HasColumnName("ended_at");
        builder.Property(m => m.AllMembersVoted).HasColumnName("all_members_voted").HasDefaultValue(false);
        builder.Property(m => m.DecisionAccepted).HasColumnName("decision_accepted");
        builder.Property(m => m.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("PREPARING");
        builder.Property(m => m.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(m => m.Status);
        builder.HasIndex(m => m.TrueConfConferenceId);
    }
}
