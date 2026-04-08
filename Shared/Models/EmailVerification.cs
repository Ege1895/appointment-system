using System;
using AppointmentSystem.Api.Shared.Models;

namespace AppointmentSystem.Api.Shared.Models
{
    public class EmailVerification : BaseEntity
    {
        public required string Email { get; set; }
        public required string CodeHash { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int AttemptCount { get; set; } = 0;
        public required string Purpose { get; set; } // Örn: "BusinessRegistration", "PasswordReset"
    }
}
