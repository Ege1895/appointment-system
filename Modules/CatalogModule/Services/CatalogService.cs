using AppointmentSystem.Api.Modules.BusinessModule.Models;
using AppointmentSystem.Api.Modules.CatalogModule.DTOs;
using AppointmentSystem.Api.Modules.PromotionModule.Models;
using AppointmentSystem.Api.Shared.Caching;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Database;
using Microsoft.EntityFrameworkCore;
using Shared.Responses;

namespace AppointmentSystem.Api.Modules.CatalogModule.Services
{
    public class CatalogService : ICatalogService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICacheService _cacheService;

        public CatalogService(ApplicationDbContext context, ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<IReadOnlyList<CategoryFilterItemDto>> GetCategoryFiltersAsync()
        {
            IReadOnlyList<CategoryFilterItemDto>? cached = await _cacheService.GetAsync<IReadOnlyList<CategoryFilterItemDto>>(CacheKeys.CATALOG_CATEGORIES);
            if (cached != null)
            {
                return cached;
            }

            IReadOnlyList<CategoryFilterItemDto> result = CatalogCategoryDefinitions.FilterItems;
            await _cacheService.SetAsync(CacheKeys.CATALOG_CATEGORIES, result, TimeSpan.FromHours(BusinessRules.CACHE_CATEGORIES_TTL_HOURS));
            return result;
        }

        public async Task<(IReadOnlyList<PublicBusinessSummaryDto> Items, PaginationMeta Meta)> SearchBusinessesAsync(
            int? categoryFlag,
            int page,
            int pageSize)
        {
            if (page < 1)
            {
                page = 1;
            }

            if (pageSize < 1)
            {
                pageSize = BusinessRules.DEFAULT_PAGE_SIZE;
            }

            if (pageSize > BusinessRules.MAX_CATALOG_PAGE_SIZE)
            {
                pageSize = BusinessRules.MAX_CATALOG_PAGE_SIZE;
            }

            string cacheVersion = await _cacheService.GetAsync<string>("catalog:version") ?? "v1";
            string cacheKey = string.Format(CacheKeys.CATALOG_SEARCH_BUSINESSES, categoryFlag ?? 0, page, pageSize) + ":" + cacheVersion;

            (IReadOnlyList<PublicBusinessSummaryDto> Items, PaginationMeta Meta)? cachedResult =
                await _cacheService.GetAsync<(IReadOnlyList<PublicBusinessSummaryDto>, PaginationMeta)>(cacheKey);

            if (cachedResult.HasValue)
            {
                return cachedResult.Value;
            }

            IQueryable<Business> query = _context.Businesses
                .AsNoTracking()
                .Where(b => b.IsActive && b.IsOpen);

            if (categoryFlag is int mask && mask > 0)
            {
                query = query.Where(b => ((int)b.Category & mask) != 0);
            }

            int total = await query.CountAsync();

            var rows = await query
                .Select(b => new
                {
                    Business = b,
                    Boost = _context.BusinessBoosts
                        .Where(bb => bb.BusinessId == b.Id && bb.IsActive && bb.ExpiresAt > DateTime.UtcNow)
                        .Select(bb => new { bb.Package.BoostScore, bb.ExpiresAt })
                        .FirstOrDefault()
                })
                .OrderByDescending(x => x.Boost != null ? x.Boost.BoostScore : 0)
                .ThenByDescending(x => x.Business.AverageRating)
                .ThenByDescending(x => x.Business.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            PaginationMeta meta = PaginationMeta.Create(page, pageSize, total);
            var items = rows.Select(x => MapWithBoost(x.Business, x.Boost != null, x.Boost?.ExpiresAt)).ToList();
            (IReadOnlyList<PublicBusinessSummaryDto>, PaginationMeta) result = (items, meta);

            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(BusinessRules.CACHE_CATALOG_TTL_MINUTES));

            return result;
        }

        public async Task<IReadOnlyList<PublicBusinessSummaryDto>> GetPopularBusinessesAsync(int take)
        {
            if (take < 1)
            {
                take = BusinessRules.DEFAULT_POPULAR_TAKE;
            }

            if (take > BusinessRules.MAX_POPULAR_TAKE)
            {
                take = BusinessRules.MAX_POPULAR_TAKE;
            }

            string cacheVersion = await _cacheService.GetAsync<string>("catalog:version") ?? "v1";
            string cacheKey = string.Format(CacheKeys.CATALOG_POPULAR_BUSINESSES, take) + ":" + cacheVersion;

            IReadOnlyList<PublicBusinessSummaryDto>? cached = await _cacheService.GetAsync<IReadOnlyList<PublicBusinessSummaryDto>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            List<PublicBusinessSummaryDto> items = await _context.Businesses
                .AsNoTracking()
                .Where(b => b.IsActive && b.IsOpen)
                .Select(b => new
                {
                    Business = b,
                    Boost = _context.BusinessBoosts
                        .Where(bb => bb.BusinessId == b.Id && bb.IsActive && bb.ExpiresAt > DateTime.UtcNow)
                        .Select(bb => new { bb.Package.BoostScore, bb.ExpiresAt })
                        .FirstOrDefault()
                })
                .OrderByDescending(x => x.Boost != null ? x.Boost.BoostScore : 0)
                .ThenByDescending(x => x.Business.AverageRating)
                .ThenByDescending(x => x.Business.CreatedAt)
                .Take(take)
                .Select(x => MapWithBoost(x.Business, x.Boost != null, x.Boost != null ? (DateTime?)x.Boost.ExpiresAt : null))
                .ToListAsync();

            IReadOnlyList<PublicBusinessSummaryDto> result = items;
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(BusinessRules.CACHE_CATALOG_TTL_MINUTES));

            return result;
        }

        private static PublicBusinessSummaryDto MapToSummary(Business b)
        {
            return MapWithBoost(b, false, null);
        }

        private static PublicBusinessSummaryDto MapWithBoost(Business b, bool isPromoted, DateTime? expiresAt)
        {
            string? thumb = b.GalleryUrls is { Count: > 0 } ? b.GalleryUrls[0] : null;

            return new PublicBusinessSummaryDto
            {
                BusinessId = b.Id,
                CompanyName = b.CompanyName,
                Subdomain = b.Subdomain,
                Category = b.Category,
                City = b.City,
                District = b.District,
                AverageRating = b.AverageRating,
                ReviewCount = b.ReviewCount,
                IsOpen = b.IsOpen,
                ThumbnailUrl = thumb,
                IsPromoted = isPromoted,
                BoostExpiresAt = expiresAt
            };
        }
    }
}
