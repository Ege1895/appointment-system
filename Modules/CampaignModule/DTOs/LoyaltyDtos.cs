using AppointmentSystem.Api.Modules.CampaignModule.Enums;

namespace AppointmentSystem.Api.Modules.CampaignModule.DTOs
{
    public class SetupLoyaltyProgramRequest
    {
        public bool IsEnabled { get; set; }
        public int PointsPerAppointment { get; set; }
        public decimal? PointsPerCurrencyUnit { get; set; }
        public decimal RedemptionRate { get; set; }
        public int MinPointsToRedeem { get; set; }
        public int? MaxPointsRedemptionPerBooking { get; set; }
        public int? ExpiryDays { get; set; }
    }

    public class LoyaltyProgramResponseDto
    {
        public Guid Id { get; set; }
        public Guid BusinessId { get; set; }
        public bool IsEnabled { get; set; }
        public int PointsPerAppointment { get; set; }
        public decimal? PointsPerCurrencyUnit { get; set; }
        public decimal RedemptionRate { get; set; }
        public int MinPointsToRedeem { get; set; }
        public int? MaxPointsRedemptionPerBooking { get; set; }
        public int? ExpiryDays { get; set; }
    }

    public class UserPointsResponseDto
    {
        public Guid BusinessId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public int CurrentPoints { get; set; }
        public int TotalEarned { get; set; }
        public int TotalRedeemed { get; set; }
        public DateTime? LastEarnedAt { get; set; }
    }

    public class LoyaltyTransactionResponseDto
    {
        public Guid Id { get; set; }
        public Guid? AppointmentId { get; set; }
        public LoyaltyTransactionType Type { get; set; }
        public int Points { get; set; }
        public int BalanceAfter { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
