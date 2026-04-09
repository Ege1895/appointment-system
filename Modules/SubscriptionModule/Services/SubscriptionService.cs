using AppointmentSystem.Api.Modules.SubscriptionModule.Models;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Api.Modules.SubscriptionModule.Services
{
    public interface ISubscriptionService
    {
        Task<List<SubscriptionPackageDto>> GetAvailablePackagesAsync();
        Task<BusinessSubscriptionDto?> GetMySubscriptionAsync(Guid businessId);
        Task<BusinessSubscriptionDto> GetOrAssignDefaultAsync(Guid businessId);
        Task AssignPackageAsync(Guid businessId, Guid packageId, DateTime? expiresAt = null);
    }

    public class SubscriptionService : ISubscriptionService
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SubscriptionPackageDto>> GetAvailablePackagesAsync()
        {
            return await _context.SubscriptionPackages
                .Where(p => p.IsPublic)
                .OrderBy(p => p.SortOrder)
                .Select(p => new SubscriptionPackageDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    PriceMonthly = p.PriceMonthly,
                    PriceYearly = p.PriceYearly,
                    MaxEmployees = p.MaxEmployees,
                    MaxBranches = p.MaxBranches,
                    MaxOfferings = p.MaxOfferings,
                    MaxMonthlyAppointments = p.MaxMonthlyAppointments,
                    CampaignsEnabled = p.CampaignsEnabled,
                    LoyaltyEnabled = p.LoyaltyEnabled,
                    ExportEnabled = p.ExportEnabled,
                    AnalyticsEnabled = p.AnalyticsEnabled,
                    PromotionEnabled = p.PromotionEnabled,
                    SortOrder = p.SortOrder
                })
                .ToListAsync();
        }

        public async Task<BusinessSubscriptionDto?> GetMySubscriptionAsync(Guid businessId)
        {
            var sub = await _context.BusinessSubscriptions
                .Include(bs => bs.Package)
                .FirstOrDefaultAsync(bs => bs.BusinessId == businessId);

            if (sub == null) return null;

            return MapToDto(sub);
        }

        public async Task<BusinessSubscriptionDto> GetOrAssignDefaultAsync(Guid businessId)
        {
            var existing = await GetMySubscriptionAsync(businessId);
            if (existing != null) return existing;

            // Find "Free" package
            var freePackage = await _context.SubscriptionPackages
                .FirstOrDefaultAsync(p => p.Id == SubscriptionConstants.FREE_PACKAGE_ID);

            if (freePackage == null)
            {
                throw new InvalidOperationException("Default 'Free' subscription package not found in database.");
            }

            var newSub = new BusinessSubscription
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                PackageId = freePackage.Id,
                StartedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _context.BusinessSubscriptions.AddAsync(newSub);
            await _context.SaveChangesAsync();

            // Reload with package details
            newSub.Package = freePackage;
            return MapToDto(newSub);
        }

        public async Task AssignPackageAsync(Guid businessId, Guid packageId, DateTime? expiresAt = null)
        {
            var package = await _context.SubscriptionPackages.FindAsync(packageId);
            if (package == null) throw new ArgumentException("Invalid package ID");

            var existing = await _context.BusinessSubscriptions
                .FirstOrDefaultAsync(bs => bs.BusinessId == businessId);

            if (existing != null)
            {
                existing.PackageId = packageId;
                existing.ExpiresAt = expiresAt;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                await _context.BusinessSubscriptions.AddAsync(new BusinessSubscription
                {
                    Id = Guid.NewGuid(),
                    BusinessId = businessId,
                    PackageId = packageId,
                    StartedAt = DateTime.UtcNow,
                    ExpiresAt = expiresAt,
                    IsActive = true
                });
            }

            await _context.SaveChangesAsync();
        }

        private static BusinessSubscriptionDto MapToDto(BusinessSubscription sub)
        {
            return new BusinessSubscriptionDto
            {
                BusinessId = sub.BusinessId,
                PackageId = sub.PackageId,
                PackageName = sub.Package.Name,
                StartedAt = sub.StartedAt,
                ExpiresAt = sub.ExpiresAt,
                IsActive = sub.IsActive,
                BillingCycle = sub.BillingCycle,
                MaxEmployees = sub.Package.MaxEmployees,
                MaxBranches = sub.Package.MaxBranches,
                CampaignsEnabled = sub.Package.CampaignsEnabled,
                LoyaltyEnabled = sub.Package.LoyaltyEnabled,
                AnalyticsEnabled = sub.Package.AnalyticsEnabled
            };
        }
    }
}
