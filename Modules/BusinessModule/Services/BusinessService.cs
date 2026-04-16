using AppointmentSystem.Api.Modules.BusinessModule.DTOs;
using AppointmentSystem.Api.Modules.BusinessModule.Models;
using AppointmentSystem.Api.Modules.BusinessModule.Enums;
using AppointmentSystem.Api.Modules.AppointmentModule.Enums;
using AppointmentSystem.Api.Modules.AppointmentModule.Models;
using AppointmentSystem.Api.Modules.SubscriptionModule.DTOs;
using AppointmentSystem.Api.Modules.SubscriptionModule.Services;
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
        private readonly ISubscriptionService _subscriptionService;

        public BusinessService(ApplicationDbContext context, ICurrentUserService currentUserService, ISubscriptionService subscriptionService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _subscriptionService = subscriptionService;
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

            var subscriptionInfo = await _subscriptionService.GetSubscriptionInfoAsync(businessId);
            return MapToResponse(business, subscriptionInfo);
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

        public async Task<DeactivationResponseDto> DeactivateAsync()
        {
            Guid businessId = GetBusinessId();
            Business? business = await _context.Businesses.FirstOrDefaultAsync(b => b.Id == businessId);

            if (business == null)
            {
                throw new NotFoundException(ErrorMessages.BUSINESS_NOT_FOUND);
            }

            business.IsActive = false;
            await _context.SaveChangesAsync();

            return new DeactivationResponseDto
            {
                IsActive = false,
                Message = "İşletme hesabı donduruldu. Katalogda görünmeyecektir."
            };
        }

        public async Task<DeactivationResponseDto> ReactivateAsync()
        {
            Guid businessId = GetBusinessId();
            Business? business = await _context.Businesses.FirstOrDefaultAsync(b => b.Id == businessId);

            if (business == null)
            {
                throw new NotFoundException(ErrorMessages.BUSINESS_NOT_FOUND);
            }

            business.IsActive = true;
            await _context.SaveChangesAsync();

            return new DeactivationResponseDto
            {
                IsActive = true,
                Message = "İşletme hesabı tekrar aktif edildi."
            };
        }

        public async Task DeleteAccountAsync(DeleteBusinessAccountRequest request)
        {
            Guid businessId = GetBusinessId();
            Business? business = await _context.Businesses.FirstOrDefaultAsync(b => b.Id == businessId);

            if (business == null)
            {
                throw new NotFoundException(ErrorMessages.BUSINESS_NOT_FOUND);
            }

            // Doğrulama
            bool isPasswordValid = PasswordHasher.Verify(request.Password, business.PasswordHash);
            if (!isPasswordValid)
            {
                throw new BusinessException(ErrorMessages.WRONG_PASSWORD_CONFIRMATION);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Randevuları İptal Et
                List<AppointmentStatus> cancelableStatuses = new List<AppointmentStatus>
                {
                    AppointmentStatus.PendingApproval,
                    AppointmentStatus.Confirmed,
                    AppointmentStatus.PendingTransfer
                };

                List<Appointment> appointments = await _context.Appointments
                    .Where(a => a.BusinessId == businessId && cancelableStatuses.Contains(a.Status))
                    .ToListAsync();

                foreach (Appointment appointment in appointments)
                {
                    appointment.Status = AppointmentStatus.Cancelled;
                    
                    _context.AppointmentNotes.Add(new AppointmentNote
                    {
                        Id = Guid.NewGuid(),
                        AppointmentId = appointment.Id,
                        BusinessId = businessId,
                        Content = BusinessRules.BUSINESS_CLOSED_APPOINTMENT_NOTE,
                        AuthorId = "SYSTEM",
                        AuthorRole = "System",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                // 2. İşletmeyi Soft-Delete Yap ve Anonimleştir
                business.IsDeleted = true;
                business.IsActive = false;
                business.Email = $"deleted_{business.Id}{BusinessRules.DELETED_EMAIL_SUFFIX}";
                business.ContactPhone = null;
                business.OwnerFirstName = BusinessRules.DELETED_FIRST_NAME;
                business.OwnerLastName = BusinessRules.DELETED_LAST_NAME;
                
                // 3. Refresh Token'ları Geçersiz Kıl
                business.RefreshTokenHash = null;
                business.RefreshTokenExpiry = null;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private BusinessResponseDto MapToResponse(Business business, SubscriptionInfo? sub = null)
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
                EmployeeCapacity = sub?.MaxEmployees ?? business.EmployeeCapacity,
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
                CreatedAt = business.CreatedAt,
                Subscription = sub
            };
        }
    }
}
