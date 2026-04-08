using AppointmentSystem.Api.Modules.AppointmentModule.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSystem.Api.Modules.AppointmentModule.Configurations
{
    public class AppointmentMessageConfiguration : IEntityTypeConfiguration<AppointmentMessage>
    {
        public void Configure(EntityTypeBuilder<AppointmentMessage> builder)
        {
            builder.ToTable("AppointmentMessages");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Content)
                .IsRequired()
                .HasMaxLength(1000);

            builder.HasOne(m => m.Appointment)
                .WithMany() // Can add collection to Appointment if needed later
                .HasForeignKey(m => m.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(m => m.AppointmentId);
            builder.HasIndex(m => m.BusinessId);

            // Global Query Filter for Soft Delete
            builder.HasQueryFilter(m => !m.IsDeleted);
        }
    }
}
