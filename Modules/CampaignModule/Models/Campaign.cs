using AppointmentSystem.Api.Modules.BusinessModule.Models;
using AppointmentSystem.Api.Modules.CampaignModule.Enums;
using AppointmentSystem.Api.Shared.Models;

namespace AppointmentSystem.Api.Modules.CampaignModule.Models
{
    public class Campaign : BaseEntity
    {
        public Guid BusinessId { get; set; }
        public Business Business { get; set; } = null!;

        public required string Name { get; set; }
        public string? Code { get; set; } // Null ise otomatik uygulanır

        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; } // Yüzde indirimleri için üst sınır
        public decimal? MinBookingPrice { get; set; }

        // JSONB olarak saklanacak
        public List<Guid>? OfferingIds { get; set; }

        public ApplicableTo ApplicableTo { get; set; } = ApplicableTo.All;

        public DateTime StartsAt { get; set; }
        public DateTime? ExpiresAt { get; set; }

        public int? MaxUses { get; set; }
        public int CurrentUses { get; set; } = 0;
    }
}
