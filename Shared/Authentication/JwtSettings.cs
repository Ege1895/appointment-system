namespace AppointmentSystem.Api.Shared.Authentication
{
    public class JwtSettings
    {
        public required string SecretKey { get; set; }
        public int ExpirationMinutes { get; set; } = 60;
        public required string Issuer { get; set; }
        public required string Audience { get; set; }
    }
}