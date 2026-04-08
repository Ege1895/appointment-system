using System;
using AppointmentSystem.Api.Modules.UserModule.Models;
using AppointmentSystem.Api.Shared.Interfaces;
using AppointmentSystem.Api.Shared.Models;

namespace AppointmentSystem.Api.Modules.BusinessModule.Models
{
    public class BusinessCustomerBlock : BaseEntity, IMustHaveTenant
    {
        public Guid BusinessId { get; set; }
        public Business Business { get; set; } = null!;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string? Reason { get; set; }
        public DateTime BlockedAt { get; set; } = DateTime.UtcNow;
    }
}
