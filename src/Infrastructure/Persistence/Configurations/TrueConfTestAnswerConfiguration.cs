using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class TrueConfTestAnswerConfiguration : IEntityTypeConfiguration<TrueConfTestAnswer>
{
    public void Configure(EntityTypeBuilder<TrueConfTestAnswer> builder)
    {
        builder.ToTable("trueconf_test_answer");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");

        builder.Property(a => a.QuestionId).HasColumnName("question_id").IsRequired();
        builder.Property(a => a.UserName).HasColumnName("user_name").HasMaxLength(100).IsRequired();
        builder.Property(a => a.VoteValue).HasColumnName("vote_value").HasMaxLength(20).IsRequired();
        builder.Property(a => a.VotedAt).HasColumnName("voted_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(a => a.QuestionId);

        builder.HasOne(a => a.Question)
            .WithMany(q => q.Answers)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
