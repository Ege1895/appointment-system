using AppointmentSystem.Api.Modules.BusinessModule.Models;
using AppointmentSystem.Api.Shared.Models;

namespace AppointmentSystem.Api.Modules.CampaignModule.Models
{
    public class LoyaltyProgram : BaseEntity
    {
        public Guid BusinessId { get; set; }
        public Business Business { get; set; } = null!;

        public bool IsEnabled { get; set; } = false;

        // Puan Kazanma Kuralları
        public int PointsPerAppointment { get; set; } = 0; // Her randevu için sabit puan
        public decimal? PointsPerCurrencyUnit { get; set; } // Örn: Her 1 TL için X puan

        // Puan Bozdurma Kuralları
        public decimal RedemptionRate { get; set; } // Örn: 100 Puan = 10 TL ise RedemptionRate 0.1 olur
        public int MinPointsToRedeem { get; set; } = 0;
        public int? MaxPointsRedemptionPerBooking { get; set; }

        public int? ExpiryDays { get; set; } // Puanların geçerlilik süresi (gün)
    }
}
