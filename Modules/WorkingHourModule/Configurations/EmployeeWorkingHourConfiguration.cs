using AppointmentSystem.Api.Modules.WorkingHourModule.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppointmentSystem.Api.Shared.Constants;

namespace AppointmentSystem.Api.Modules.WorkingHourModule.Configurations
{
    public class EmployeeWorkingHourConfiguration : IEntityTypeConfiguration<EmployeeWorkingHour>
    {
        public void Configure(EntityTypeBuilder<EmployeeWorkingHour> builder)
        {
            builder.ToTable("EmployeeWorkingHours");

            builder.HasKey(w => w.Id);

            builder.Property(w => w.DayOfWeek)
                .IsRequired();

            builder.Property(w => w.IsDayOff)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(w => w.StartTime)
                .IsRequired();

            builder.Property(w => w.EndTime)
                .IsRequired();

            builder.Property(w => w.BreakStartTime)
                .IsRequired(false);

            builder.Property(w => w.BreakEndTime)
                .IsRequired(false);

            // Bir çalışanın aynı gün için sadece 1 çalışma saati kaydı olabilir
            builder.HasIndex(w => new { w.EmployeeId, w.DayOfWeek })
                .IsUnique()
                .HasFilter(DatabaseConstants.NOT_DELETED_FILTER);

            builder.HasOne(w => w.Employee)
                .WithMany()
                .HasForeignKey(w => w.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(w => w.Business)
                .WithMany()
                .HasForeignKey(w => w.BusinessId)
                .OnDelete(DeleteBehavior.Restrict);

            // Global Query Filter for Soft Delete
            builder.HasQueryFilter(w => !w.IsDeleted);
        }
    }
}
