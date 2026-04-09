using AppointmentSystem.Api.Modules.AppointmentModule.Enums;
using AppointmentSystem.Api.Modules.AppointmentModule.Models;
using AppointmentSystem.Api.Modules.CampaignModule.DTOs;
using AppointmentSystem.Api.Modules.CampaignModule.Enums;
using AppointmentSystem.Api.Modules.CampaignModule.Models;
using AppointmentSystem.Api.Modules.SubscriptionModule.Services;
using AppointmentSystem.Api.Shared.Authentication;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Database;
using AppointmentSystem.Api.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Api.Modules.CampaignModule.Services
{
    public class LoyaltyService : ILoyaltyService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ISubscriptionEnforcementService _enforcementService;

        public LoyaltyService(ApplicationDbContext context, ICurrentUserService currentUserService, ISubscriptionEnforcementService enforcementService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _enforcementService = enforcementService;
        }

        private Guid GetBusinessId() => _currentUserService.TenantId ?? throw new BusinessException(ErrorMessages.BUSINESS_CONTEXT_NOT_FOUND);
        private Guid GetUserId() => Guid.Parse(_currentUserService.UserId ?? throw new BusinessException(ErrorMessages.UNAUTHORIZED_ACCESS));

        public async Task<LoyaltyProgramResponseDto> SetupProgramAsync(SetupLoyaltyProgramRequest request)
        {
            Guid businessId = GetBusinessId();
            
            if (request.IsEnabled)
            {
                await _enforcementService.EnsureFeatureEnabledAsync(businessId, "Loyalty");
            }

            LoyaltyProgram? program = await _context.LoyaltyPrograms.FirstOrDefaultAsync(lp => lp.BusinessId == businessId);

            if (program == null)
            {
                program = new LoyaltyProgram { BusinessId = businessId };
                await _context.LoyaltyPrograms.AddAsync(program);
            }

            program.IsEnabled = request.IsEnabled;
            program.PointsPerAppointment = request.PointsPerAppointment;
            program.PointsPerCurrencyUnit = request.PointsPerCurrencyUnit;
            program.RedemptionRate = request.RedemptionRate;
            program.MinPointsToRedeem = request.MinPointsToRedeem;
            program.MaxPointsRedemptionPerBooking = request.MaxPointsRedemptionPerBooking;
            program.ExpiryDays = request.ExpiryDays;
            program.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToDto(program);
        }

        public async Task<LoyaltyProgramResponseDto?> GetProgramAsync(Guid businessId)
        {
            LoyaltyProgram? program = await _context.LoyaltyPrograms
                .AsNoTracking()
                .FirstOrDefaultAsync(lp => lp.BusinessId == businessId);

            return program != null ? MapToDto(program) : null;
        }

        public async Task<UserPointsResponseDto> GetUserPointsAsync(Guid businessId)
        {
            Guid userId = GetUserId();
            UserLoyaltyPoints? points = await _context.UserLoyaltyPoints
                .AsNoTracking()
                .FirstOrDefaultAsync(ulp => ulp.BusinessId == businessId && ulp.UserId == userId);

            if (points == null)
            {
                string businessName = await _context.Businesses
                    .Where(b => b.Id == businessId)
                    .Select(b => b.CompanyName)
                    .FirstOrDefaultAsync() ?? "İşletme";

                return new UserPointsResponseDto
                {
                    BusinessId = businessId,
                    BusinessName = businessName,
                    CurrentPoints = 0,
                    TotalEarned = 0,
                    TotalRedeemed = 0
                };
            }

            return new UserPointsResponseDto
            {
                BusinessId = points.BusinessId,
                BusinessName = points.Business.CompanyName,
                CurrentPoints = points.CurrentPoints,
                TotalEarned = points.TotalEarned,
                TotalRedeemed = points.TotalRedeemed,
                LastEarnedAt = points.LastEarnedAt
            };
        }

        public async Task<List<LoyaltyTransactionResponseDto>> GetTransactionHistoryAsync(Guid businessId)
        {
            Guid userId = GetUserId();
            List<LoyaltyTransaction> transactions = await _context.LoyaltyTransactions
                .Where(lt => lt.BusinessId == businessId && lt.UserId == userId)
                .OrderByDescending(lt => lt.CreatedAt)
                .ToListAsync();

            return transactions.Select(lt => new LoyaltyTransactionResponseDto
            {
                Id = lt.Id,
                AppointmentId = lt.AppointmentId,
                Type = lt.Type,
                Points = lt.Points,
                BalanceAfter = lt.BalanceAfter,
                Notes = lt.Notes,
                CreatedAt = lt.CreatedAt
            }).ToList();
        }

        public async Task EarnPointsForAppointmentAsync(Guid appointmentId)
        {
            // Bu metod internal/worker tarafından çağrılır, TenantId check bypass edilmeli (veya doğru set edilmeli)
            Appointment? app = await _context.Appointments
                .Include(a => a.Offering)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (app == null || app.Status != AppointmentStatus.Completed) return;

            LoyaltyProgram? program = await _context.LoyaltyPrograms
                .FirstOrDefaultAsync(lp => lp.BusinessId == app.BusinessId);

            if (program == null || !program.IsEnabled) return;

            int earnedPoints = program.PointsPerAppointment;
            if (program.PointsPerCurrencyUnit.HasValue && app.Offering.Price.HasValue)
            {
                earnedPoints += (int)(app.Offering.Price.Value * program.PointsPerCurrencyUnit.Value);
            }

            if (earnedPoints <= 0) return;

            UserLoyaltyPoints? userPoints = await _context.UserLoyaltyPoints
                .FirstOrDefaultAsync(ulp => ulp.BusinessId == app.BusinessId && ulp.UserId == app.UserId);

            if (userPoints == null)
            {
                userPoints = new UserLoyaltyPoints
                {
                    BusinessId = app.BusinessId,
                    UserId = app.UserId,
                    CurrentPoints = 0,
                    TotalEarned = 0,
                    TotalRedeemed = 0
                };
                await _context.UserLoyaltyPoints.AddAsync(userPoints);
            }

            userPoints.CurrentPoints += earnedPoints;
            userPoints.TotalEarned += earnedPoints;
            userPoints.LastEarnedAt = DateTime.UtcNow;
            userPoints.UpdatedAt = DateTime.UtcNow;

            LoyaltyTransaction transaction = new LoyaltyTransaction
            {
                Id = Guid.NewGuid(),
                BusinessId = app.BusinessId,
                UserId = app.UserId,
                AppointmentId = app.Id,
                Type = LoyaltyTransactionType.Earned,
                Points = earnedPoints,
                BalanceAfter = userPoints.CurrentPoints,
                Notes = $"Randevu tamamlandı: {app.Offering.Name}",
                CreatedAt = DateTime.UtcNow
            };

            await _context.LoyaltyTransactions.AddAsync(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<decimal> CalculateRedemptionDiscountAsync(Guid businessId, Guid userId, int pointsToRedeem)
        {
            if (pointsToRedeem <= 0) return 0;

            LoyaltyProgram? program = await _context.LoyaltyPrograms
                .AsNoTracking()
                .FirstOrDefaultAsync(lp => lp.BusinessId == businessId);

            if (program == null || !program.IsEnabled)
                throw new BusinessException(ErrorMessages.LOYALTY_PROGRAM_DISABLED);

            if (pointsToRedeem < program.MinPointsToRedeem)
                throw new BusinessException($"{ErrorMessages.LOYALTY_MIN_POINTS_REQUIRED} {program.MinPointsToRedeem}");

            if (program.MaxPointsRedemptionPerBooking.HasValue && pointsToRedeem > program.MaxPointsRedemptionPerBooking.Value)
                throw new BusinessException(ErrorMessages.LOYALTY_MAX_POINTS_EXCEEDED);

            return pointsToRedeem * program.RedemptionRate;
        }

        public async Task ConsumePointsAsync(Guid businessId, Guid userId, Guid appointmentId, int pointsToRedeem)
        {
            if (pointsToRedeem <= 0) return;

            // Optimistic concurrency check via atomic update in SQL
            // Ancak burada EF Core kullandığımız için direkt SQL veya Transaction içinde kontrol yapacağız.
            // AppointmentService.CreateAsync zaten SerializationLevel.Serializable kullanıyor.

            UserLoyaltyPoints? userPoints = await _context.UserLoyaltyPoints
                .FirstOrDefaultAsync(ulp => ulp.BusinessId == businessId && ulp.UserId == userId);

            if (userPoints == null || userPoints.CurrentPoints < pointsToRedeem)
                throw new BusinessException(ErrorMessages.LOYALTY_INSUFFICIENT_POINTS);

            userPoints.CurrentPoints -= pointsToRedeem;
            userPoints.TotalRedeemed += pointsToRedeem;
            userPoints.UpdatedAt = DateTime.UtcNow;

            LoyaltyTransaction transaction = new LoyaltyTransaction
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                UserId = userId,
                AppointmentId = appointmentId,
                Type = LoyaltyTransactionType.Redeemed,
                Points = -pointsToRedeem,
                BalanceAfter = userPoints.CurrentPoints,
                Notes = "Randevu oluşturulurken puan kullanıldı.",
                CreatedAt = DateTime.UtcNow
            };

            await _context.LoyaltyTransactions.AddAsync(transaction);
            // SaveChangesAsync burada çağrılmıyor, AppointmentService.CreateAsync'in SaveChanges'ına güveniyoruz.
        }

        private static LoyaltyProgramResponseDto MapToDto(LoyaltyProgram lp)
        {
            return new LoyaltyProgramResponseDto
            {
                Id = lp.Id,
                BusinessId = lp.BusinessId,
                IsEnabled = lp.IsEnabled,
                PointsPerAppointment = lp.PointsPerAppointment,
                PointsPerCurrencyUnit = lp.PointsPerCurrencyUnit,
                RedemptionRate = lp.RedemptionRate,
                MinPointsToRedeem = lp.MinPointsToRedeem,
                MaxPointsRedemptionPerBooking = lp.MaxPointsRedemptionPerBooking,
                ExpiryDays = lp.ExpiryDays
            };
        }
    }
}
