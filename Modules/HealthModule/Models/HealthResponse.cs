namespace AppointmentSystem.Api.Modules.HealthModule.Models
{
    public class HealthResponse
    {
        public string Status { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }

        public string Service { get; set; } = string.Empty;
    }
}