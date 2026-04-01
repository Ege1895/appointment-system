using AppointmentSystem.Api.Modules.WorkingHourModule.DTOs;

namespace AppointmentSystem.Api.Modules.WorkingHourModule.Services
{
    public interface IWorkingHourService
    {
        Task SetWorkingHoursAsync(Guid employeeId, List<WorkingHourDto> workingHours);
        Task<List<WorkingHourDto>> GetWorkingHoursAsync(Guid employeeId);
    }
}
