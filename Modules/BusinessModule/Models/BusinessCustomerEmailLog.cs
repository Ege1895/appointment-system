using System;
using AppointmentSystem.Api.Shared.Models;
using AppointmentSystem.Api.Shared.Interfaces;
using AppointmentSystem.Api.Modules.UserModule.Models;

namespace AppointmentSystem.Api.Modules.BusinessModule.Models
{
    public class BusinessCustomerEmailLog : BaseEntity, IMustHaveTenant
    {
        public Guid BusinessId { get; set; }
        public Business Business { get; set; } = null!;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string Subject { get; set; } = string.Empty;
        public string MessageBody { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
