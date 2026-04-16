using AppointmentSystem.Api.Modules.PromotionModule.Models;
using AppointmentSystem.Api.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Api.Modules.PromotionModule.Data
{
    public static class PromotionSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.BoostPackages.AnyAsync()) return;

            var packages = new List<BoostPackage>
            {
                new BoostPackage
                {
                    Id = Guid.NewGuid(),
                    Name = "7-Day Catalog Boost",
                    DurationDays = 7,
                    Price = 19.99m,
                    BoostScore = 100,
                    SortOrder = 1,
                    IsActive = true
                },
                new BoostPackage
                {
                    Id = Guid.NewGuid(),
                    Name = "30-Day Catalog Boost",
                    DurationDays = 30,
                    Price = 59.99m,
                    BoostScore = 150,
                    SortOrder = 2,
                    IsActive = true
                }
            };

            await context.BoostPackages.AddRangeAsync(packages);
            await context.SaveChangesAsync();
        }
    }
}
