namespace AppointmentSystem.Api.Modules.Auth.DTOs
{
    public class UnifiedAuthResponseDto
    {
        public required string Token { get; set; }
        public required string RefreshToken { get; set; }
        public required string Role { get; set; }
        public Guid? BusinessId { get; set; }
    }
}
