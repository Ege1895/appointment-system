using AppointmentSystem.Api.Modules.PromotionModule.DTOs;
using AppointmentSystem.Api.Modules.PromotionModule.Models;
using AppointmentSystem.Api.Modules.SubscriptionModule.Services;
using AppointmentSystem.Api.Shared.Caching;
using AppointmentSystem.Api.Shared.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppointmentSystem.Api.Modules.PromotionModule.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICacheService _cacheService;
        private readonly ISubscriptionEnforcementService _enforcementService;

        public PromotionService(
            ApplicationDbContext context,
            ICacheService cacheService,
            ISubscriptionEnforcementService enforcementService)
        {
            _context = context;
            _cacheService = cacheService;
            _enforcementService = enforcementService;
        }

        public async Task<List<BoostPackageDto>> GetBoostPackagesAsync()
        {
            return await _context.BoostPackages
                .Where(p => p.IsActive)
                .OrderBy(p => p.SortOrder)
                .Select(p => new BoostPackageDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    DurationDays = p.DurationDays,
                    Price = p.Price,
                    BoostScore = p.BoostScore,
                    IsActive = p.IsActive
                })
                .ToListAsync();
        }

        public async Task<BusinessBoostResponseDto> PurchaseBoostAsync(Guid businessId, PurchaseBoostRequest request)
        {
            // 1. Abonelik Kontrolü
            await _enforcementService.EnsureFeatureEnabledAsync(businessId, "Promotion");

            // 2. Paket Kontrolü
            var package = await _context.BoostPackages.FindAsync(request.BoostPackageId);
            if (package == null || !package.IsActive)
            {
                throw new ArgumentException("Geçersiz veya pasif boost paketi.");
            }

            // 3. Mevcut Boost Kontrolü (Uzatma mantığı)
            var existingBoost = await _context.BusinessBoosts
                .FirstOrDefaultAsync(bb => bb.BusinessId == businessId && bb.IsActive);

            BusinessBoost boost;
            if (existingBoost != null && existingBoost.ExpiresAt > DateTime.UtcNow)
            {
                // Uzat (Mevcut bitiş tarihine ekle)
                existingBoost.ExpiresAt = existingBoost.ExpiresAt.AddDays(package.DurationDays);
                existingBoost.BoostPackageId = package.Id; // En son alınan paketin skoru geçerli olsun (opsiyonel tercih)
                existingBoost.UpdatedAt = DateTime.UtcNow;
                boost = existingBoost;
            }
            else
            {
                // Yeni oluştur (Mevcut pasifleri sil veya güncelle yerine yeni temiz kayıt)
                boost = new BusinessBoost
                {
                    Id = Guid.NewGuid(),
                    BusinessId = businessId,
                    BoostPackageId = package.Id,
                    StartsAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(package.DurationDays),
                    IsActive = true
                };
                await _context.BusinessBoosts.AddAsync(boost);
            }

            await _context.SaveChangesAsync();

            // 4. Katalog Cache'ini Temizle
            await _cacheService.RemoveByPrefixAsync("catalog:");

            return new BusinessBoostResponseDto
            {
                BoostId = boost.Id,
                PackageName = package.Name,
                StartsAt = boost.StartsAt,
                ExpiresAt = boost.ExpiresAt,
                IsCurrentlyActive = true,
                BoostScore = package.BoostScore
            };
        }

        public async Task<BusinessBoostResponseDto?> GetActiveBoostAsync(Guid businessId)
        {
            var boost = await _context.BusinessBoosts
                .Include(bb => bb.Package)
                .FirstOrDefaultAsync(bb => bb.BusinessId == businessId && bb.IsActive && bb.ExpiresAt > DateTime.UtcNow);

            if (boost == null) return null;

            return new BusinessBoostResponseDto
            {
                BoostId = boost.Id,
                PackageName = boost.Package.Name,
                StartsAt = boost.StartsAt,
                ExpiresAt = boost.ExpiresAt,
                IsCurrentlyActive = true,
                BoostScore = boost.Package.BoostScore
            };
        }

        public async Task<bool> IsBusinessBoostedAsync(Guid businessId)
        {
            return await _context.BusinessBoosts
                .AnyAsync(bb => bb.BusinessId == businessId && bb.IsActive && bb.ExpiresAt > DateTime.UtcNow);
        }
    }
}
