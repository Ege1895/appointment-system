using AppointmentSystem.Api.Shared.Models;

namespace AppointmentSystem.Api.Modules.BusinessModule.Models
{
    /// <summary>
    /// Henüz sisteme kayıtlı olmayan ancak bir işletme tarafından içe aktarılan müşteri adayları.
    /// Davetiye gönderimi ve takip için kullanılır.
    /// </summary>
    public class PendingCustomerInvitation : BaseEntity
    {
        public Guid BusinessId { get; set; }
        public Business Business { get; set; } = null!;

        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? PhoneNumber { get; set; }

        public DateTime InvitedAt { get; set; } = DateTime.UtcNow;
        public bool IsAccepted { get; set; } = false;
    }
}
