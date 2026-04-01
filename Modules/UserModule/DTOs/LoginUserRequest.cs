namespace AppointmentSystem.Api.Modules.UserModule.DTOs
{
    public class LoginUserRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
