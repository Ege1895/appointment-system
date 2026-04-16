using AppointmentSystem.Api.Modules.BusinessModule.Models;
using AppointmentSystem.Api.Modules.SubscriptionModule.Enums;
using AppointmentSystem.Api.Shared.Models;

namespace AppointmentSystem.Api.Modules.SubscriptionModule.Models
{
    public class BusinessSubscription : BaseEntity
    {
        public Guid BusinessId { get; set; }
        public Business Business { get; set; } = null!;

        public Guid PackageId { get; set; }
        public SubscriptionPackage Package { get; set; } = null!;

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; } // Null = Sonsuz (Örn: Lifetime veya Default Free)
        
        public bool IsActive { get; set; } = true;
        public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;
    }
}
