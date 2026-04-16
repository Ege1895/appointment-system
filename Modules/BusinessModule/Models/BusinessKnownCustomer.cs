using AppointmentSystem.Api.Shared.Models;
using AppointmentSystem.Api.Modules.UserModule.Models;

namespace AppointmentSystem.Api.Modules.BusinessModule.Models
{
    /// <summary>
    /// İşletmenin tanıdığı (sistemde kayıtlı olan) müşterileri temsil eden varlık.
    /// Randevu almamış olsalar bile işletme bu müşterileri takip edebilir.
    /// </summary>
    public class BusinessKnownCustomer : BaseEntity
    {
        public Guid BusinessId { get; set; }
        public Business Business { get; set; } = null!;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        
        // Ekstra bilgiler gerekirse buraya eklenebilir.
        // Örneğin: İşletmeye özel notlar, müşteri tipi vb.
    }
}
