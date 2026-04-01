using AppointmentSystem.Api.Modules.AppointmentModule.DTOs;

namespace AppointmentSystem.Api.Modules.AppointmentModule.Services
{
    public interface IAvailabilityService
    {
        Task<List<TimeSpan>> GetAvailableSlotsAsync(Guid employeeId, DateTime date, Guid offeringId);
    }
}
