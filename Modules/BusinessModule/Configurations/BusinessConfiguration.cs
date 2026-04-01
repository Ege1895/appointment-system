using AppointmentSystem.Api.Modules.BusinessModule.Models;
using AppointmentSystem.Api.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSystem.Api.Modules.BusinessModule.Configurations
{
    public class BusinessConfiguration : IEntityTypeConfiguration<Business>
    {
        public void Configure(EntityTypeBuilder<Business> builder)
        {
            builder.HasKey(b => b.Id);
            builder.Property(b => b.CompanyName).IsRequired().HasMaxLength(BusinessRules.MAX_COMPANY_NAME_LENGTH);
            builder.Property(b => b.Subdomain).IsRequired().HasMaxLength(BusinessRules.MAX_SUBDOMAIN_LENGTH);
            builder.HasIndex(b => b.Subdomain).IsUnique().HasFilter(DatabaseConstants.NOT_DELETED_FILTER); 
            
            builder.Property(b => b.OwnerFirstName).IsRequired().HasMaxLength(BusinessRules.MAX_NAME_LENGTH);
            builder.Property(b => b.OwnerLastName).IsRequired().HasMaxLength(BusinessRules.MAX_NAME_LENGTH);
            builder.Property(b => b.Email).IsRequired().HasMaxLength(BusinessRules.MAX_EMAIL_LENGTH);
            builder.HasIndex(b => b.Email).IsUnique().HasFilter(DatabaseConstants.NOT_DELETED_FILTER); // For Login queries
            
            builder.Property(b => b.PasswordHash).IsRequired();
            builder.Property(b => b.ContactPhone).IsRequired().HasMaxLength(BusinessRules.MAX_PHONE_LENGTH);
            
            builder.Property(b => b.EmployeeCapacity).IsRequired().HasDefaultValue(BusinessRules.DEFAULT_EMPLOYEE_CAPACITY);

            builder.Property(b => b.AverageRating)
                .HasPrecision(3, 2)
                .HasDefaultValue(0m);

            builder.Property(b => b.ReviewCount)
                .IsRequired()
                .HasDefaultValue(0);
            
            builder.Property(b => b.City).HasMaxLength(BusinessRules.MAX_CITY_LENGTH);
            builder.Property(b => b.District).HasMaxLength(BusinessRules.MAX_DISTRICT_LENGTH);
            builder.Property(b => b.Neighborhood).HasMaxLength(BusinessRules.MAX_NEIGHBORHOOD_LENGTH);
            builder.Property(b => b.Description).HasMaxLength(BusinessRules.MAX_DESCRIPTION_LENGTH);
            builder.Property(b => b.IsOpen).HasDefaultValue(true);
            
            builder.Property(b => b.CreatedAt).IsRequired();
            builder.Property(b => b.IsActive).IsRequired().HasDefaultValue(true);

            // Enterprise Refresh Token Config
            builder.Property(b => b.RefreshTokenHash).IsRequired(false).HasMaxLength(500);
            builder.Property(b => b.RefreshTokenExpiry).IsRequired(false);
            builder.Property(b => b.RefreshTokenUsed).HasDefaultValue(false);
            
            builder.HasIndex(b => b.RefreshTokenHash); // Ensure high performance lookups
            builder.HasIndex(b => new { b.AverageRating, b.ReviewCount });

            // Global Query Filter for Soft Delete
            builder.HasQueryFilter(b => !b.IsDeleted);
        }
    }
}
