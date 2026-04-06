using AppointmentSystem.Api.Modules.EmployeeModule.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSystem.Api.Modules.EmployeeModule.Configurations
{
    public class EmployeeOfferingConfiguration : IEntityTypeConfiguration<EmployeeOffering>
    {
        public void Configure(EntityTypeBuilder<EmployeeOffering> builder)
        {
            builder.ToTable("EmployeeOfferings");

            builder.HasKey(eo => eo.Id);

            builder.HasOne(eo => eo.Employee)
                .WithMany()
                .HasForeignKey(eo => eo.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(eo => eo.Offering)
                .WithMany()
                .HasForeignKey(eo => eo.OfferingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(eo => new { eo.EmployeeId, eo.OfferingId }).IsUnique();
            builder.HasIndex(eo => eo.BusinessId);

            // Global Query Filter for Soft Delete
            builder.HasQueryFilter(eo => !eo.IsDeleted);
        }
    }
}
