using AppointmentSystem.Api.Modules.SubscriptionModule.Models;
using AppointmentSystem.Api.Shared.Caching;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Database;
using AppointmentSystem.Api.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Api.Modules.SubscriptionModule.Services
{
    public interface ISubscriptionEnforcementService
    {
        Task EnsureCanAddEmployeeAsync(Guid businessId);
        Task EnsureCanAddBranchAsync(Guid businessId);
        Task EnsureCanAddOfferingAsync(Guid businessId);
        Task EnsureFeatureEnabledAsync(Guid businessId, string featureName);
        Task<SubscriptionPackage> GetActivePackageAsync(Guid businessId);
    }

    public class SubscriptionEnforcementService : ISubscriptionEnforcementService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICacheService _cacheService;
        private const string CACHE_KEY_PREFIX = "sub_package_";
        private static readonly TimeSpan CACHE_TTL = TimeSpan.FromMinutes(5);

        public SubscriptionEnforcementService(ApplicationDbContext context, ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<SubscriptionPackage> GetActivePackageAsync(Guid businessId)
        {
            string cacheKey = $"{CACHE_KEY_PREFIX}{businessId}";
            var cached = await _cacheService.GetAsync<SubscriptionPackage>(cacheKey);
            if (cached != null) return cached;

            var sub = await _context.BusinessSubscriptions
                .Include(bs => bs.Package)
                .AsNoTracking()
                .FirstOrDefaultAsync(bs => bs.BusinessId == businessId && bs.IsActive);

            SubscriptionPackage package;

            // If subscription is expired or missing, fallback to Free package rules
            if (sub == null || (sub.ExpiresAt.HasValue && sub.ExpiresAt.Value < DateTime.UtcNow))
            {
                package = await _context.SubscriptionPackages
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == SubscriptionConstants.FREE_PACKAGE_ID)
                    ?? throw new InvalidOperationException("Default 'Free' package not found.");
            }
            else
            {
                package = sub.Package;
            }

            await _cacheService.SetAsync(cacheKey, package, CACHE_TTL);
            return package;
        }

        public async Task EnsureCanAddEmployeeAsync(Guid businessId)
        {
            var package = await GetActivePackageAsync(businessId);
            if (package.MaxEmployees == 0) return; // Unlimited

            int currentCount = await _context.Employees.CountAsync(e => e.BusinessId == businessId);
            if (currentCount >= package.MaxEmployees)
            {
                throw new SubscriptionLimitException($"Employee limit reached ({package.MaxEmployees}). Please upgrade your plan.", "Pro");
            }
        }

        public async Task EnsureCanAddBranchAsync(Guid businessId)
        {
            var package = await GetActivePackageAsync(businessId);
            if (package.MaxBranches == 0) return;

            int currentCount = await _context.Branches.CountAsync(b => b.BusinessId == businessId);
            if (currentCount >= package.MaxBranches)
            {
                throw new SubscriptionLimitException($"Branch limit reached ({package.MaxBranches}). Please upgrade your plan.", "Pro");
            }
        }

        public async Task EnsureCanAddOfferingAsync(Guid businessId)
        {
            var package = await GetActivePackageAsync(businessId);
            if (package.MaxOfferings == 0) return;

            int currentCount = await _context.Offerings.CountAsync(o => o.BusinessId == businessId);
            if (currentCount >= package.MaxOfferings)
            {
                throw new SubscriptionLimitException($"Offering limit reached ({package.MaxOfferings}). Please upgrade your plan.", "Pro");
            }
        }

        public async Task EnsureFeatureEnabledAsync(Guid businessId, string featureName)
        {
            var package = await GetActivePackageAsync(businessId);
            bool enabled = featureName.ToLowerInvariant() switch
            {
                "campaigns" => package.CampaignsEnabled,
                "loyalty" => package.LoyaltyEnabled,
                "export" => package.ExportEnabled,
                "analytics" => package.AnalyticsEnabled,
                "promotion" => package.PromotionEnabled,
                _ => true
            };

            if (!enabled)
            {
                throw new SubscriptionLimitException($"The '{featureName}' feature is not available in your current plan. Please upgrade.", "Pro");
            }
        }
    }
}
