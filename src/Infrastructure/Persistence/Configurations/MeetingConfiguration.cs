using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Enums;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class MeetingConfiguration : IEntityTypeConfiguration<Meeting>
{
    public void Configure(EntityTypeBuilder<Meeting> builder)
    {
        builder.ToTable("meetings");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");

        builder.Property(m => m.MeetingNumber).HasColumnName("meeting_number").HasMaxLength(50);
        builder.Property(m => m.MeetingForm).HasColumnName("meeting_form").HasMaxLength(20).IsRequired();
        builder.Property(m => m.Status).HasColumnName("status").HasMaxLength(50).HasDefaultValue(MeetingStatus.DRAFT);
        builder.Property(m => m.VotingStartAt).HasColumnName("voting_start_at");
        builder.Property(m => m.VotingEndAt).HasColumnName("voting_end_at");
        builder.Property(m => m.CreatedBy).HasColumnName("created_by");
        builder.Property(m => m.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(m => m.MeetingNumber);
        builder.HasIndex(m => m.Status);

        builder.HasOne(m => m.Creator)
            .WithMany()
            .HasForeignKey(m => m.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
