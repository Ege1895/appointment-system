namespace AppointmentSystem.Api.Modules.BusinessModule.DTOs
{
    public class DeactivationResponseDto
    {
        public bool IsActive { get; set; }
        public required string Message { get; set; }
    }
}
