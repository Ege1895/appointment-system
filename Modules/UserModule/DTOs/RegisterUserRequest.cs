namespace AppointmentSystem.Api.Modules.UserModule.DTOs
{
    public class RegisterUserRequest
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string PhoneNumber { get; set; }

        public required bool KvkkAccepted { get; set; }
        public required bool PrivacyPolicyAccepted { get; set; }
    }
}
