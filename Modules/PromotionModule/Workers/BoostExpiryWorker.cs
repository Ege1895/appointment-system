using AppointmentSystem.Api.Modules.PromotionModule.Models;
using AppointmentSystem.Api.Shared.Caching;
using AppointmentSystem.Api.Shared.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AppointmentSystem.Api.Modules.PromotionModule.Workers
{
    public class BoostExpiryWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BoostExpiryWorker> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

        public BoostExpiryWorker(IServiceProvider serviceProvider, ILogger<BoostExpiryWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Boost Expiry Worker is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessExpirationsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing boost expirations.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task ProcessExpirationsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

            var now = DateTime.UtcNow;

            // Süresi dolmuş aktif boostları bul (Global Query Filter'dan dolayı IgnoreQueryFilters gerekebilir 
            // ama burada sadece IsActive olanları kapatacağımız için normal sorgu yeterli olabilir. 
            // Ancak QueryFilter'da bb.IsActive == true varsa sadece onları görürüz, bu da tam ihtiyacımız olan şey.)
            
            var expiredBoosts = await context.BusinessBoosts
                .Where(bb => bb.IsActive && bb.ExpiresAt < now)
                .ToListAsync();

            if (expiredBoosts.Any())
            {
                _logger.LogInformation("Found {Count} expired boosts to deactivate.", expiredBoosts.Count);

                foreach (var boost in expiredBoosts)
                {
                    boost.IsActive = false;
                    boost.UpdatedAt = now;
                }

                await context.SaveChangesAsync();

                // Katalog önbelleğini temizle (Sıralama değiştiği için)
                await cacheService.RemoveByPrefixAsync("catalog:");
                
                _logger.LogInformation("Successfully deactivated expired boosts and cleared catalog cache.");
            }
        }
    }
}
