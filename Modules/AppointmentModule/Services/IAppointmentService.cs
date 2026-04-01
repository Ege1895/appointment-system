using AppointmentSystem.Api.Modules.AppointmentModule.DTOs;

namespace AppointmentSystem.Api.Modules.AppointmentModule.Services
{
    public interface IAppointmentService
    {
        Task<AppointmentResponseDto> CreateAsync(CreateAppointmentRequest request);
        Task<List<AppointmentResponseDto>> GetMyAsync();
        Task<List<AppointmentResponseDto>> GetMyUpcomingAsync(int limit);
        Task<List<AppointmentResponseDto>> GetBusinessAppointmentsAsync(DateTime? date);
        Task CancelAsync(Guid appointmentId);
        
        Task AcceptAsync(Guid appointmentId);
        Task DeclineAsync(Guid appointmentId);
        Task TransferAsync(Guid appointmentId, Guid newEmployeeId);
        Task RespondToTransferAsync(Guid appointmentId, bool isAccepted);
        Task RequestRescheduleAsync(Guid appointmentId, DateTime newStartTimeUtc);
        Task RespondToRescheduleAsync(Guid appointmentId, bool isAccepted);
    }
}
