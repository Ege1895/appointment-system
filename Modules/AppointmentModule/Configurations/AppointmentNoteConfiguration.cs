using AppointmentSystem.Api.Modules.AppointmentModule.Models;
using AppointmentSystem.Api.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSystem.Api.Modules.AppointmentModule.Configurations
{
    public class AppointmentNoteConfiguration : IEntityTypeConfiguration<AppointmentNote>
    {
        public void Configure(EntityTypeBuilder<AppointmentNote> builder)
        {
            builder.ToTable("AppointmentNotes");

            builder.HasKey(n => n.Id);

            builder.Property(n => n.Content)
                .IsRequired()
                .HasMaxLength(BusinessRules.MAX_NOTE_CONTENT_LENGTH);

            builder.HasOne(n => n.Appointment)
                .WithMany() // Appointments often don't need a back-reference to all notes if we query them separately
                .HasForeignKey(n => n.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(n => n.AppointmentId);
            builder.HasIndex(n => n.BusinessId);

            // Global Query Filter for Soft Delete
            builder.HasQueryFilter(n => !n.IsDeleted);
        }
    }
}
