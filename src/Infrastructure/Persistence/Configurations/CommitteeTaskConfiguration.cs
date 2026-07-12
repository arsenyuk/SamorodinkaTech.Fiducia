using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Enums;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class CommitteeTaskConfiguration : IEntityTypeConfiguration<CommitteeTask>
{
    public void Configure(EntityTypeBuilder<CommitteeTask> builder)
    {
        builder.ToTable("committee_tasks");

        builder.HasKey(ct => ct.Id);
        builder.Property(ct => ct.Id).HasColumnName("id").UseIdentityAlwaysColumn();

        builder.Property(ct => ct.CommitteeId).HasColumnName("committee_id").IsRequired();
        builder.Property(ct => ct.AgendaQuestionId).HasColumnName("agenda_question_id");
        builder.Property(ct => ct.TaskDescription).HasColumnName("task_description").IsRequired();
        builder.Property(ct => ct.DeadlineAt).HasColumnName("deadline_at").IsRequired();
        builder.Property(ct => ct.Status).HasColumnName("status").HasMaxLength(50)
            .HasConversion<string>().HasDefaultValue(CommitteeTaskStatus.IN_WORK);
        builder.Property(ct => ct.CreatedBy).HasColumnName("created_by");
        builder.Property(ct => ct.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(ct => ct.CommitteeId);
        builder.HasIndex(ct => ct.Status);
        builder.HasIndex(ct => ct.DeadlineAt);

        builder.HasOne(ct => ct.Committee)
            .WithMany(c => c.Tasks)
            .HasForeignKey(ct => ct.CommitteeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ct => ct.AgendaQuestion)
            .WithMany(aq => aq.CommitteeTasks)
            .HasForeignKey(ct => ct.AgendaQuestionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(ct => ct.Creator)
            .WithMany()
            .HasForeignKey(ct => ct.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
