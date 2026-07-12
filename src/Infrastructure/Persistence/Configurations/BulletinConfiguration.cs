using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence.Configurations;

public class BulletinConfiguration : IEntityTypeConfiguration<Bulletin>
{
    public void Configure(EntityTypeBuilder<Bulletin> builder)
    {
        builder.ToTable("bulletins");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).HasColumnName("id").UseIdentityAlwaysColumn();

        builder.Property(b => b.AgendaQuestionId).HasColumnName("agenda_question_id").IsRequired();
        builder.Property(b => b.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(b => b.VoteValue).HasColumnName("vote_value").HasMaxLength(15).IsRequired();
        builder.Property(b => b.SpecialOpinion).HasColumnName("special_opinion");
        builder.Property(b => b.SignatureType).HasColumnName("signature_type").HasMaxLength(10).IsRequired();
        builder.Property(b => b.SignatureValue).HasColumnName("signature_value").IsRequired();
        builder.Property(b => b.SignedAt).HasColumnName("signed_at").IsRequired();
        builder.Property(b => b.IsCancelled).HasColumnName("is_cancelled").HasDefaultValue(false);
        builder.Property(b => b.CancellationReason).HasColumnName("cancellation_reason");

        builder.HasIndex(b => b.AgendaQuestionId);
        builder.HasIndex(b => b.UserId);

        builder.HasOne(b => b.AgendaQuestion)
            .WithMany(aq => aq.Bulletins)
            .HasForeignKey(b => b.AgendaQuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.User)
            .WithMany(u => u.Bulletins)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
