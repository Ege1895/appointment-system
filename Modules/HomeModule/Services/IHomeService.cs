using AppointmentSystem.Api.Modules.HomeModule.DTOs;

namespace AppointmentSystem.Api.Modules.HomeModule.Services
{
    public interface IHomeService
    {
        Task<HomeDashboardDto> GetDashboardAsync(int popularTake, int upcomingLimit);
    }
}
