using AppointmentSystem.Api.Modules.BusinessModule.Models;
using AppointmentSystem.Api.Modules.SubscriptionModule.Enums;
using AppointmentSystem.Api.Shared.Models;

namespace AppointmentSystem.Api.Modules.SubscriptionModule.Models
{
    public class SubscriptionChangeRequest : BaseEntity
    {
        public Guid BusinessId { get; set; }
        public Business Business { get; set; } = null!;

        public Guid? FromPackageId { get; set; }
        public SubscriptionPackage? FromPackage { get; set; }

        public Guid ToPackageId { get; set; }
        public SubscriptionPackage ToPackage { get; set; } = null!;

        public SubscriptionChangeStatus Status { get; set; } = SubscriptionChangeStatus.Pending;
        public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        public string? PaymentReference { get; set; } // Örn: Stripe Session ID veya Banka Dekont No
    }
}
