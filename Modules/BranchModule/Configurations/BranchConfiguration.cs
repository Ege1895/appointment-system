using AppointmentSystem.Api.Modules.BranchModule.Models;
using AppointmentSystem.Api.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSystem.Api.Modules.BranchModule.Configurations
{
    public class BranchConfiguration : IEntityTypeConfiguration<Branch>
    {
        public void Configure(EntityTypeBuilder<Branch> builder)
        {
            builder.ToTable("Branches");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.Name)
                .IsRequired()
                .HasMaxLength(BusinessRules.MAX_BRANCH_NAME_LENGTH);

            builder.Property(b => b.Address)
                .HasMaxLength(BusinessRules.MAX_BRANCH_ADDRESS_LENGTH);

            builder.Property(b => b.City)
                .HasMaxLength(BusinessRules.MAX_CITY_LENGTH);

            builder.Property(b => b.District)
                .HasMaxLength(BusinessRules.MAX_DISTRICT_LENGTH);

            builder.Property(b => b.PhoneNumber)
                .HasMaxLength(BusinessRules.MAX_PHONE_LENGTH);

            builder.Property(b => b.IsMain)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(b => b.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Foreign Key Configuration
            builder.HasOne(b => b.Business)
                .WithMany() // Business entity might need a collection of Branches if we want bi-directional
                .HasForeignKey(b => b.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(b => b.BusinessId);
            builder.HasIndex(b => new { b.BusinessId, b.IsActive });
            builder.HasIndex(b => new { b.BusinessId, b.IsMain });

            // Global Query Filter for Soft Delete (Will be combined in DbContext if needed, 
            // but usually individual filters are also good to have here as a backup)
            builder.HasQueryFilter(b => !b.IsDeleted);
        }
    }
}
