using AppointmentSystem.Api.Modules.AppointmentModule.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSystem.Api.Modules.AppointmentModule.Configurations
{
    public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.ToTable("Appointments");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.StartTimeUtc)
                .IsRequired();

            builder.Property(a => a.EndTimeUtc)
                .IsRequired();

            builder.Property(a => a.Status)
                .IsRequired();

            // Yüksek trafik ve hızlı sorgu için indekslemeler
            builder.HasIndex(a => new { a.EmployeeId, a.StartTimeUtc, a.EndTimeUtc });
            builder.HasIndex(a => a.UserId);
            builder.HasIndex(a => a.BusinessId);

            // İlişkiler
            builder.HasOne(a => a.Business)
                .WithMany()
                .HasForeignKey(a => a.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Employee)
                .WithMany()
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Offering)
                .WithMany()
                .HasForeignKey(a => a.OfferingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.TransferredToEmployee)
                .WithMany()
                .HasForeignKey(a => a.TransferredToEmployeeId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // Global Query Filter for Soft Delete
            builder.HasQueryFilter(a => !a.IsDeleted);
        }
    }
}
