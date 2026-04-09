using AppointmentSystem.Api.Modules.CampaignModule.DTOs;
using AppointmentSystem.Api.Modules.CampaignModule.Models;

namespace AppointmentSystem.Api.Modules.CampaignModule.Services
{
    public interface ILoyaltyService
    {
        // Business Methods
        Task<LoyaltyProgramResponseDto> SetupProgramAsync(SetupLoyaltyProgramRequest request);
        Task<LoyaltyProgramResponseDto?> GetProgramAsync(Guid businessId);

        // User Methods
        Task<UserPointsResponseDto> GetUserPointsAsync(Guid businessId);
        Task<List<LoyaltyTransactionResponseDto>> GetTransactionHistoryAsync(Guid businessId);

        // Internal / Integration Methods
        Task EarnPointsForAppointmentAsync(Guid appointmentId);
        Task<decimal> CalculateRedemptionDiscountAsync(Guid businessId, Guid userId, int pointsToRedeem);
        Task ConsumePointsAsync(Guid businessId, Guid userId, Guid appointmentId, int pointsToRedeem);
    }
}
