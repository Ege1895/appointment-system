using AppointmentSystem.Api.Modules.OfferingModule.Models;
using AppointmentSystem.Api.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSystem.Api.Modules.OfferingModule.Configurations
{
    public class OfferingConfiguration : IEntityTypeConfiguration<Offering>
    {
        public void Configure(EntityTypeBuilder<Offering> builder)
        {
            builder.ToTable("Offerings");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(BusinessRules.MAX_SERVICE_NAME_LENGTH);

            builder.Property(s => s.Description)
                .IsRequired(false)
                .HasMaxLength(BusinessRules.MAX_SERVICE_DESC_LENGTH);

            builder.Property(s => s.DurationInMinutes)
                .IsRequired();

            builder.Property(s => s.PriceType)
                .IsRequired();

            builder.Property(s => s.Price)
                .HasColumnType(DatabaseConstants.TYPE_DECIMAL_MONEY)
                .IsRequired(false);

            builder.Property(s => s.IsOnline)
                .IsRequired()
                .HasDefaultValue(false);

            // Index on BusinessId for multi-tenant queries
            builder.HasIndex(s => s.BusinessId);

            // Foreign Key Configuration
            builder.HasOne(s => s.Business)
                .WithMany()
                .HasForeignKey(s => s.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);

            // Global Query Filter for Soft Delete
            builder.HasQueryFilter(s => !s.IsDeleted);
        }
    }
}
