using AppointmentSystem.Api.Modules.BusinessModule.Models;
using AppointmentSystem.Api.Modules.UserModule.Models;
using AppointmentSystem.Api.Shared.Models;

namespace AppointmentSystem.Api.Modules.CampaignModule.Models
{
    public class UserLoyaltyPoints : BaseEntity
    {
        public Guid BusinessId { get; set; }
        public Business Business { get; set; } = null!;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public int CurrentPoints { get; set; }
        public int TotalEarned { get; set; }
        public int TotalRedeemed { get; set; }

        public DateTime? LastEarnedAt { get; set; }
        
        // Optimistic Concurrency için Version ekleyebiliriz veya RowVersion kullanabiliriz
        // BaseEntity'de UpdatedAt var, ancak atomic operations için DB üzerinde direkt SQL kullanacağız.
    }
}
