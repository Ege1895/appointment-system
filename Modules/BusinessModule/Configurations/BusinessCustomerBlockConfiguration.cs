using AppointmentSystem.Api.Modules.BusinessModule.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSystem.Api.Modules.BusinessModule.Configurations
{
    public class BusinessCustomerBlockConfiguration : IEntityTypeConfiguration<BusinessCustomerBlock>
    {
        public void Configure(EntityTypeBuilder<BusinessCustomerBlock> builder)
        {
            builder.ToTable("BusinessCustomerBlocks");

            builder.HasIndex(b => new { b.BusinessId, b.UserId }).IsUnique();
            builder.HasIndex(b => b.BusinessId);

            builder.Property(b => b.Reason).HasMaxLength(500);

            builder.HasOne(b => b.Business)
                .WithMany()
                .HasForeignKey(b => b.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
