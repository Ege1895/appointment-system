namespace AppointmentSystem.Api.Modules.BusinessModule.DTOs
{
    public class BusinessAuthResponseDto
    {
        public required BusinessResponseDto Business { get; set; }
        
        public required string Token { get; set; }
        public required string RefreshToken { get; set; }
    }
}
