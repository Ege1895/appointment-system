using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppointmentSystem.Api.Modules.AppointmentModule.DTOs;

namespace AppointmentSystem.Api.Modules.AppointmentModule.Services
{
    public interface IAppointmentMessageService
    {
        Task<AppointmentMessageResponseDto> SendAsync(SendAppointmentMessageRequest request);
        Task<List<AppointmentMessageResponseDto>> GetThreadAsync(Guid appointmentId);
        Task MarkAsReadAsync(Guid appointmentId);
        Task<int> GetUnreadCountAsync();
    }
}
