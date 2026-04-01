using AppointmentSystem.Api.Modules.OfferingModule.DTOs;
using AppointmentSystem.Api.Modules.OfferingModule.Models;
using AppointmentSystem.Api.Shared.Authentication;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Database;
using AppointmentSystem.Api.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Shared.Responses;

namespace AppointmentSystem.Api.Modules.OfferingModule.Services
{
    public class OfferingService : IOfferingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public OfferingService(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        private Guid GetBusinessId()
        {
            string? userIdStr = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid businessId))
            {
                throw new BusinessException(ErrorMessages.UNAUTHORIZED_ACCESS);
            }
            return businessId;
        }

        public async Task<OfferingResponseDto> CreateAsync(CreateOfferingRequest request)
        {
            Guid businessId = GetBusinessId();

            Offering offering = new Offering
            {
                Name = request.Name,
                Description = request.Description,
                DurationInMinutes = request.DurationInMinutes,
                PriceType = request.PriceType,
                Price = request.Price,
                IsOnline = request.IsOnline,
                BusinessId = businessId
            };

            await _context.Offerings.AddAsync(offering);
            await _context.SaveChangesAsync();

            return MapToResponse(offering);
        }

        public async Task<(List<OfferingResponseDto> Items, PaginationMeta Meta)> GetAllForBusinessAsync(int page, int pageSize)
        {
            Guid businessId = GetBusinessId();

            if (page < 1)
            {
                page = 1;
            }

            if (pageSize < 1)
            {
                pageSize = BusinessRules.DEFAULT_PAGE_SIZE;
            }

            if (pageSize > BusinessRules.MAX_OFFERING_PAGE_SIZE)
            {
                pageSize = BusinessRules.MAX_OFFERING_PAGE_SIZE;
            }

            int totalCount = await _context.Offerings
                .AsNoTracking()
                .CountAsync(o => o.BusinessId == businessId && o.IsActive);

            List<Offering> offerings = await _context.Offerings
                .AsNoTracking()
                .Where(o => o.BusinessId == businessId && o.IsActive)
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            PaginationMeta meta = PaginationMeta.Create(page, pageSize, totalCount);
            return (offerings.Select(MapToResponse).ToList(), meta);
        }

        private OfferingResponseDto MapToResponse(Offering offering)
        {
            return new OfferingResponseDto
            {
                Id = offering.Id,
                Name = offering.Name,
                Description = offering.Description,
                DurationInMinutes = offering.DurationInMinutes,
                PriceType = offering.PriceType,
                Price = offering.Price,
                IsOnline = offering.IsOnline,
                IsActive = offering.IsActive
            };
        }
    }
}
