using AppointmentSystem.Api.Modules.EmployeeModule.Models;
using AppointmentSystem.Api.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSystem.Api.Modules.EmployeeModule.Configurations
{
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.BusinessId).IsRequired();
            // Critical for multi-tenant high traffic: All queries will filter by BusinessId!
            builder.HasIndex(e => e.BusinessId);
            
            builder.Property(e => e.FirstName).IsRequired().HasMaxLength(BusinessRules.MAX_NAME_LENGTH);
            builder.Property(e => e.LastName).IsRequired().HasMaxLength(BusinessRules.MAX_NAME_LENGTH);
            builder.Property(e => e.JobTitle).IsRequired().HasMaxLength(BusinessRules.MAX_JOB_TITLE_LENGTH);
            builder.Property(e => e.JobDescription).HasMaxLength(BusinessRules.MAX_JOB_DESCRIPTION_LENGTH);
            builder.Property(e => e.CreatedAt).IsRequired();

            // Isolation relationship (Cascade delete handles data cleanup implicitly)
            builder.HasOne(e => e.Business)
                .WithMany()
                .HasForeignKey(e => e.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(e => e.Branch)
                .WithMany()
                .HasForeignKey(e => e.BranchId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(e => new { e.BusinessId, e.BranchId });

            // Global Query Filter for Soft Delete
            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
}
