using AppointmentSystem.Api.Modules.BusinessModule.Enums;

namespace AppointmentSystem.Api.Modules.BusinessModule.DTOs
{
    public class RegisterBusinessRequest
    {
        public BusinessCategory Category { get; set; }
        
        public required string CompanyName { get; set; }
        public required string Subdomain { get; set; }
        public required string OwnerFirstName { get; set; }
        public required string OwnerLastName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string ContactPhone { get; set; }

        public required bool KvkkAccepted { get; set; }
        public required bool PrivacyPolicyAccepted { get; set; }
    }
}
