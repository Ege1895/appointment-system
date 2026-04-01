using AppointmentSystem.Api.Modules.FeedbackModule.Models;
using AppointmentSystem.Api.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSystem.Api.Modules.FeedbackModule.Configurations
{
    public class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
    {
        public void Configure(EntityTypeBuilder<Feedback> builder)
        {
            builder.ToTable("Feedbacks");

            builder.HasKey(f => f.Id);

            builder.Property(f => f.Category)
                .IsRequired();

            builder.Property(f => f.Text)
                .IsRequired()
                .HasMaxLength(BusinessRules.MAX_FEEDBACK_TEXT_LENGTH);

            builder.Property(f => f.SubmittedByRole)
                .IsRequired()
                .HasMaxLength(32);

            builder.HasIndex(f => f.Category);
            builder.HasIndex(f => f.BusinessId);
            builder.HasIndex(f => f.UserId);
            builder.HasIndex(f => f.CreatedAt);

            builder.HasOne(f => f.Business)
                .WithMany()
                .HasForeignKey(f => f.BusinessId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasQueryFilter(f => !f.IsDeleted);
        }
    }
}
