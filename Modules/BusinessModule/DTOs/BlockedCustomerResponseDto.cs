using System;

namespace AppointmentSystem.Api.Modules.BusinessModule.DTOs
{
    public class BlockedCustomerResponseDto
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public DateTime BlockedAt { get; set; }
    }
}
