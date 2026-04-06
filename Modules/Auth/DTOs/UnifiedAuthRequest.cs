using AppointmentSystem.Api.Modules.Auth.Enums;

namespace AppointmentSystem.Api.Modules.Auth.DTOs
{
    public class UnifiedAuthRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required AccountType AccountType { get; set; }
    }
}
