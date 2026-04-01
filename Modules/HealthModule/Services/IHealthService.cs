using AppointmentSystem.Api.Modules.HealthModule.Models;

namespace AppointmentSystem.Api.Modules.HealthModule.Services
{
    public interface IHealthService
    {
        HealthResponse GetHealth();
    }
}