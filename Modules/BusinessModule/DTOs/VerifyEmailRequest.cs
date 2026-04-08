namespace AppointmentSystem.Api.Modules.BusinessModule.DTOs
{
    public class VerifyEmailRequest
    {
        public required string Email { get; set; }
        public required string Code { get; set; }
    }
}
