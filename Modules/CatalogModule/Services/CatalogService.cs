using AppointmentSystem.Api.Modules.BusinessModule.Models;
using AppointmentSystem.Api.Modules.CatalogModule.DTOs;
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

            string cacheKey = string.Format(CacheKeys.CATALOG_SEARCH_BUSINESSES, categoryFlag ?? 0, page, pageSize);

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

            List<Business> rows = await query
                .OrderByDescending(b => b.AverageRating)
                .ThenByDescending(b => b.ReviewCount)
                .ThenByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            PaginationMeta meta = PaginationMeta.Create(page, pageSize, total);
            (IReadOnlyList<PublicBusinessSummaryDto>, PaginationMeta) result = (rows.Select(MapToSummary).ToList(), meta);

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

            string cacheKey = string.Format(CacheKeys.CATALOG_POPULAR_BUSINESSES, take);

            IReadOnlyList<PublicBusinessSummaryDto>? cached = await _cacheService.GetAsync<IReadOnlyList<PublicBusinessSummaryDto>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            List<Business> rows = await _context.Businesses
                .AsNoTracking()
                .Where(b => b.IsActive && b.IsOpen)
                .OrderByDescending(b => b.AverageRating)
                .ThenByDescending(b => b.ReviewCount)
                .ThenByDescending(b => b.CreatedAt)
                .Take(take)
                .ToListAsync();

            IReadOnlyList<PublicBusinessSummaryDto> result = rows.Select(MapToSummary).ToList();
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(BusinessRules.CACHE_CATALOG_TTL_MINUTES));

            return result;
        }

        private static PublicBusinessSummaryDto MapToSummary(Business b)
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
                ThumbnailUrl = thumb
            };
        }
    }
}
