using AppointmentSystem.Api.Modules.UserModule.Models;
using AppointmentSystem.Api.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSystem.Api.Modules.UserModule.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(BusinessRules.MAX_NAME_LENGTH);

            builder.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(BusinessRules.MAX_NAME_LENGTH);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(BusinessRules.MAX_EMAIL_LENGTH);

            builder.Property(u => u.PasswordHash)
                .IsRequired();

            builder.Property(u => u.PhoneNumber)
                .IsRequired()
                .HasMaxLength(BusinessRules.MAX_PHONE_LENGTH);

            // Global Index: Email must be unique across the entire platform
            builder.HasIndex(u => u.Email)
                .IsUnique()
                .HasFilter(DatabaseConstants.NOT_DELETED_FILTER);

            // Enterprise Refresh Token Config
            builder.Property(u => u.RefreshTokenHash)
                .IsRequired(false)
                .HasMaxLength(500);

            builder.Property(u => u.RefreshTokenExpiry)
                .IsRequired(false);

            builder.Property(u => u.RefreshTokenUsed)
                .HasDefaultValue(false);

            builder.HasIndex(u => u.RefreshTokenHash); // High performance lookups

            // Global Query Filter for Soft Delete
            builder.HasQueryFilter(u => !u.IsDeleted);
        }
    }
}
