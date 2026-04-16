using AppointmentSystem.Api.Modules.CampaignModule.Enums;

namespace AppointmentSystem.Api.Modules.CampaignModule.Models
{
    public class LoyaltyTransaction
    {
        public Guid Id { get; set; }
        
        public Guid BusinessId { get; set; }
        public Guid UserId { get; set; }
        public Guid? AppointmentId { get; set; }

        public LoyaltyTransactionType Type { get; set; }
        public int Points { get; set; } // Kazanılan (+) veya Harcanan (-)
        public int BalanceAfter { get; set; }
        
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
