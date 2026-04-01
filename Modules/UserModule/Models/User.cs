using AppointmentSystem.Api.Shared.Models;

namespace AppointmentSystem.Api.Modules.UserModule.Models
{
    public class User : BaseEntity
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public required string PhoneNumber { get; set; }

        public string? RefreshTokenHash { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
        public bool RefreshTokenUsed { get; set; } = false;
    }
}
