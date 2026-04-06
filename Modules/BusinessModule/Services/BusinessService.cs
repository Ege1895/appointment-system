using AppointmentSystem.Api.Modules.BusinessModule.DTOs;
using AppointmentSystem.Api.Modules.BusinessModule.Models;
using AppointmentSystem.Api.Modules.BusinessModule.Enums;
using AppointmentSystem.Api.Shared.Authentication;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Database;
using AppointmentSystem.Api.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Api.Modules.BusinessModule.Services
{
    public class BusinessService : IBusinessService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public BusinessService(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        private Guid GetBusinessId()
        {
            if (_currentUserService.Role != Roles.Business.ToString() || !_currentUserService.TenantId.HasValue)
            {
                throw new BusinessException(ErrorMessages.UNAUTHORIZED_ACCESS);
            }
            return _currentUserService.TenantId.Value;
        }

        public async Task<BusinessResponseDto> GetProfileAsync()
        {
            Guid businessId = GetBusinessId();
            Business? business = await _context.Businesses.FirstOrDefaultAsync(b => b.Id == businessId);

            if (business == null)
            {
                throw new NotFoundException(ErrorMessages.BUSINESS_NOT_FOUND);
            }

            return MapToResponse(business);
        }

        public async Task UpdateProfileAsync(UpdateBusinessProfileRequest request)
        {
            Guid businessId = GetBusinessId();
            Business? business = await _context.Businesses.FirstOrDefaultAsync(b => b.Id == businessId);

            if (business == null)
            {
                throw new NotFoundException(ErrorMessages.BUSINESS_NOT_FOUND);
            }

            business.Category = request.Category;
            business.City = request.City;
            business.District = request.District;
            business.Neighborhood = request.Neighborhood;
            business.Description = request.Description;
            business.IsOpen = request.IsOpen;
            business.WorkingHourStart = request.WorkingHourStart;
            business.WorkingHourEnd = request.WorkingHourEnd;
            business.WorkingDays = request.WorkingDays;
            business.GalleryUrls = request.GalleryUrls ?? new List<string>();

            await _context.SaveChangesAsync();
        }

        private BusinessResponseDto MapToResponse(Business business)
        {
            return new BusinessResponseDto
            {
                BusinessId = business.Id,
                CompanyName = business.CompanyName,
                Subdomain = business.Subdomain,
                OwnerFirstName = business.OwnerFirstName,
                OwnerLastName = business.OwnerLastName,
                Email = business.Email,
                ContactPhone = business.ContactPhone,
                EmployeeCapacity = business.EmployeeCapacity,
                Category = business.Category,
                City = business.City,
                District = business.District,
                Neighborhood = business.Neighborhood,
                Description = business.Description,
                IsOpen = business.IsOpen,
                WorkingHourStart = business.WorkingHourStart,
                WorkingHourEnd = business.WorkingHourEnd,
                WorkingDays = business.WorkingDays,
                GalleryUrls = business.GalleryUrls,
                CreatedAt = business.CreatedAt
            };
        }
    }
}
