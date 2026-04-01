namespace AppointmentSystem.Api.Modules.UserModule.DTOs
{
    public class UserAuthResponseDto
    {
        public required UserResponseDto User { get; set; }
        public required string Token { get; set; }
        public required string RefreshToken { get; set; }
    }
}
