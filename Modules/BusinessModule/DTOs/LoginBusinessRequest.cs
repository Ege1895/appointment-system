namespace AppointmentSystem.Api.Modules.BusinessModule.DTOs
{
    public class LoginBusinessRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
