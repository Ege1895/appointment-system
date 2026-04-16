using AppointmentSystem.Api.Modules.SubscriptionModule.Models;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Api.Modules.SubscriptionModule.Data
{
    public static class SubscriptionSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.SubscriptionPackages.AnyAsync()) return;

            var packages = new List<SubscriptionPackage>
            {
                new SubscriptionPackage
                {
                    Id = SubscriptionConstants.FREE_PACKAGE_ID,
                    Name = SubscriptionConstants.FREE_PACKAGE_NAME,
                    PriceMonthly = 0,
                    MaxEmployees = 2,
                    MaxBranches = 1,
                    MaxOfferings = 5,
                    MaxMonthlyAppointments = 30,
                    CampaignsEnabled = false,
                    LoyaltyEnabled = false,
                    ExportEnabled = false,
                    AnalyticsEnabled = false,
                    PromotionEnabled = false,
                    IsPublic = true,
                    SortOrder = 1
                },
                new SubscriptionPackage
                {
                    Id = SubscriptionConstants.PRO_PACKAGE_ID,
                    Name = SubscriptionConstants.PRO_PACKAGE_NAME,
                    PriceMonthly = 500,
                    PriceYearly = 5000,
                    MaxEmployees = 20,
                    MaxBranches = 5,
                    MaxOfferings = 50,
                    MaxMonthlyAppointments = 0, // Unlimited
                    CampaignsEnabled = true,
                    LoyaltyEnabled = true,
                    ExportEnabled = true,
                    AnalyticsEnabled = true,
                    PromotionEnabled = true,
                    IsPublic = true,
                    SortOrder = 2
                },
                new SubscriptionPackage
                {
                    Id = SubscriptionConstants.ENTERPRISE_PACKAGE_ID,
                    Name = SubscriptionConstants.ENTERPRISE_PACKAGE_NAME,
                    PriceMonthly = 2000,
                    PriceYearly = 20000,
                    MaxEmployees = 0,
                    MaxBranches = 0,
                    MaxOfferings = 0,
                    MaxMonthlyAppointments = 0,
                    CampaignsEnabled = true,
                    LoyaltyEnabled = true,
                    ExportEnabled = true,
                    AnalyticsEnabled = true,
                    PromotionEnabled = true,
                    IsPublic = true,
                    SortOrder = 3
                }
            };

            await context.SubscriptionPackages.AddRangeAsync(packages);
            await context.SaveChangesAsync();
        }
    }
}
