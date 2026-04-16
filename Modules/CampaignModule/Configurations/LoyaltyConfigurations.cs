using AppointmentSystem.Api.Modules.CampaignModule.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSystem.Api.Modules.CampaignModule.Configurations
{
    public class LoyaltyProgramConfiguration : IEntityTypeConfiguration<LoyaltyProgram>
    {
        public void Configure(EntityTypeBuilder<LoyaltyProgram> builder)
        {
            builder.ToTable("LoyaltyPrograms");
            builder.HasKey(lp => lp.Id);

            builder.HasIndex(lp => lp.BusinessId).IsUnique();
            
            builder.Property(lp => lp.RedemptionRate).HasPrecision(18, 4);
            builder.Property(lp => lp.PointsPerCurrencyUnit).HasPrecision(18, 4);
        }
    }

    public class UserLoyaltyPointsConfiguration : IEntityTypeConfiguration<UserLoyaltyPoints>
    {
        public void Configure(EntityTypeBuilder<UserLoyaltyPoints> builder)
        {
            builder.ToTable("UserLoyaltyPoints");
            builder.HasKey(ulp => ulp.Id);

            builder.HasIndex(ulp => new { ulp.BusinessId, ulp.UserId }).IsUnique();
        }
    }

    public class LoyaltyTransactionConfiguration : IEntityTypeConfiguration<LoyaltyTransaction>
    {
        public void Configure(EntityTypeBuilder<LoyaltyTransaction> builder)
        {
            builder.ToTable("LoyaltyTransactions");
            builder.HasKey(lt => lt.Id);

            builder.HasIndex(lt => new { lt.BusinessId, lt.UserId, lt.CreatedAt });
        }
    }
}
