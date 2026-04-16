using AppointmentSystem.Api.Modules.SubscriptionModule.DTOs;
using AppointmentSystem.Api.Modules.SubscriptionModule.Enums;
using AppointmentSystem.Api.Modules.SubscriptionModule.Models;
using AppointmentSystem.Api.Shared.Caching;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Database;
using AppointmentSystem.Api.Shared.Notifications;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Api.Modules.SubscriptionModule.Services
{
    public interface ISubscriptionService
    {
        Task<List<SubscriptionPackageDto>> GetAvailablePackagesAsync();
        Task<BusinessSubscriptionDto?> GetMySubscriptionAsync(Guid businessId);
        Task<BusinessSubscriptionDto> GetOrAssignDefaultAsync(Guid businessId);
        Task AssignPackageAsync(Guid businessId, Guid packageId, DateTime? expiresAt = null);
        Task<UpgradePackageResponseDto> RequestUpgradeAsync(Guid businessId, UpgradePackageRequest request);
        Task<UpgradePackageResponseDto> ConfirmUpgradeAsync(Guid changeRequestId);
        Task CancelUpgradeRequestAsync(Guid changeRequestId);
        Task<SubscriptionInfo?> GetSubscriptionInfoAsync(Guid businessId);
    }

    public class SubscriptionService : ISubscriptionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ICacheService _cacheService;

        public SubscriptionService(ApplicationDbContext context, IEmailService emailService, ICacheService cacheService)
        {
            _context = context;
            _emailService = emailService;
            _cacheService = cacheService;
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

        public async Task<UpgradePackageResponseDto> RequestUpgradeAsync(Guid businessId, UpgradePackageRequest request)
        {
            var toPackage = await _context.SubscriptionPackages.FindAsync(request.ToPackageId);
            if (toPackage == null || !toPackage.IsPublic)
            {
                throw new ArgumentException("Geçersiz paket seçimi.");
            }

            var currentSub = await _context.BusinessSubscriptions
                .Include(s => s.Package)
                .FirstOrDefaultAsync(s => s.BusinessId == businessId);

            if (currentSub != null && currentSub.PackageId == request.ToPackageId)
            {
                throw new ArgumentException("Zaten bu pakete sahipsiniz.");
            }

            var changeRequest = new SubscriptionChangeRequest
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                FromPackageId = currentSub?.PackageId,
                ToPackageId = request.ToPackageId,
                BillingCycle = request.BillingCycle,
                Status = SubscriptionChangeStatus.Confirmed, // Demo modunda anında onay
                RequestedAt = DateTime.UtcNow,
                ConfirmedAt = DateTime.UtcNow
            };

            await _context.SubscriptionChangeRequests.AddAsync(changeRequest);

            // Aktivasyon (Demo Modu)
            DateTime newExpiresAt = request.BillingCycle == BillingCycle.Monthly 
                ? DateTime.UtcNow.AddMonths(1) 
                : DateTime.UtcNow.AddYears(1);

            if (currentSub != null)
            {
                currentSub.PackageId = request.ToPackageId;
                currentSub.BillingCycle = request.BillingCycle;
                currentSub.ExpiresAt = newExpiresAt;
                currentSub.StartedAt = DateTime.UtcNow;
                currentSub.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                await _context.BusinessSubscriptions.AddAsync(new BusinessSubscription
                {
                    Id = Guid.NewGuid(),
                    BusinessId = businessId,
                    PackageId = request.ToPackageId,
                    BillingCycle = request.BillingCycle,
                    StartedAt = DateTime.UtcNow,
                    ExpiresAt = newExpiresAt,
                    IsActive = true
                });
            }

            await _context.SaveChangesAsync();

            // Önbellek Temizleme
            await _cacheService.RemoveAsync($"sub_package_{businessId}");

            // E-posta Bildirimi
            var businessOwner = await _context.Businesses
                .Where(b => b.Id == businessId)
                .Select(b => new { b.Email, b.OwnerFirstName })
                .FirstOrDefaultAsync();

            if (businessOwner != null)
            {
                await _emailService.SendAsync(
                    businessOwner.Email,
                    "Abonelik Paketiniz Yükseltildi",
                    $"Merhaba {businessOwner.OwnerFirstName}, aboneliğiniz başarıyla {toPackage.Name} paketine yükseltildi. Yeni limitleriniz ve özellikleriniz anında aktif edilmiştir.");
            }

            return new UpgradePackageResponseDto
            {
                ChangeRequestId = changeRequest.Id,
                FromPackageName = currentSub?.Package.Name ?? "Yok",
                ToPackageName = toPackage.Name,
                Status = "Confirmed",
                ActivatedImmediately = true,
                NewExpiresAt = newExpiresAt
            };
        }

        public async Task<UpgradePackageResponseDto> ConfirmUpgradeAsync(Guid changeRequestId)
        {
            // Gelecekte ödeme webhook'ları için kullanılacak
            throw new NotImplementedException();
        }

        public async Task CancelUpgradeRequestAsync(Guid changeRequestId)
        {
            var request = await _context.SubscriptionChangeRequests
                .FirstOrDefaultAsync(r => r.Id == changeRequestId);

            if (request == null || request.Status != SubscriptionChangeStatus.Pending)
            {
                throw new InvalidOperationException("İptal edilecek bekleyen bir talep bulunamadı.");
            }

            request.Status = SubscriptionChangeStatus.Cancelled;
            request.CancelledAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<SubscriptionInfo?> GetSubscriptionInfoAsync(Guid businessId)
        {
            var sub = await _context.BusinessSubscriptions
                .Include(bs => bs.Package)
                .AsNoTracking()
                .FirstOrDefaultAsync(bs => bs.BusinessId == businessId);

            if (sub == null) return null;

            return new SubscriptionInfo
            {
                PackageName = sub.Package.Name,
                ExpiresAt = sub.ExpiresAt,
                IsActive = sub.IsActive,
                MaxEmployees = sub.Package.MaxEmployees,
                CampaignsEnabled = sub.Package.CampaignsEnabled,
                LoyaltyEnabled = sub.Package.LoyaltyEnabled,
                AnalyticsEnabled = sub.Package.AnalyticsEnabled
            };
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
