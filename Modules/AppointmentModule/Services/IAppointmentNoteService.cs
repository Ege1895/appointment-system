using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppointmentSystem.Api.Modules.AppointmentModule.DTOs;

namespace AppointmentSystem.Api.Modules.AppointmentModule.Services
{
    public interface IAppointmentNoteService
    {
        Task<AppointmentNoteResponseDto> AddNoteAsync(CreateAppointmentNoteRequest request);
        Task<List<AppointmentNoteResponseDto>> GetNotesAsync(Guid appointmentId);
        Task UpdateNoteAsync(Guid noteId, UpdateAppointmentNoteRequest request);
        Task DeleteNoteAsync(Guid noteId);
    }
}
