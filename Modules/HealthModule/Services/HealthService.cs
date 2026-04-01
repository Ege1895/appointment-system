using AppointmentSystem.Api.Modules.HealthModule.Models;
using AppointmentSystem.Api.Shared.Constants;

namespace AppointmentSystem.Api.Modules.HealthModule.Services
{
    public class HealthService : IHealthService
    {
        public HealthResponse GetHealth()
        {
            return new HealthResponse
            {
                Status = HealthConstants.STATUS_OK,
                Timestamp = DateTime.UtcNow,
                Service = HealthConstants.SERVICE_NAME
            };
        }
    }
}