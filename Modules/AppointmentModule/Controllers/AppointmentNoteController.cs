using AppointmentSystem.Api.Modules.AppointmentModule.DTOs;
using AppointmentSystem.Api.Modules.AppointmentModule.Services;
using AppointmentSystem.Api.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppointmentSystem.Api.Shared.Constants;

namespace AppointmentSystem.Api.Modules.AppointmentModule.Controllers
{
    [ApiController]
    [Route("api/v1/appointments/{appointmentId:guid}/notes")]
    [Authorize(Roles = "Business,User")] // Business and Employee (User role)
    public class AppointmentNoteController : ControllerBase
    {
        private readonly IAppointmentNoteService _noteService;

        public AppointmentNoteController(IAppointmentNoteService noteService)
        {
            _noteService = noteService;
        }

        [HttpPost]
        public async Task<IActionResult> AddNote(Guid appointmentId, [FromBody] CreateAppointmentNoteRequest request)
        {
            // Ensure ID in route matches body
            request.AppointmentId = appointmentId;
            AppointmentNoteResponseDto response = await _noteService.AddNoteAsync(request);
            return Ok(ApiResponse<AppointmentNoteResponseDto>.SuccessResponse(response));
        }

        [HttpGet]
        public async Task<IActionResult> GetNotes(Guid appointmentId)
        {
            List<AppointmentNoteResponseDto> response = await _noteService.GetNotesAsync(appointmentId);
            return Ok(ApiResponse<List<AppointmentNoteResponseDto>>.SuccessResponse(response));
        }

        [HttpPut("{noteId:guid}")]
        public async Task<IActionResult> UpdateNote(Guid noteId, [FromBody] UpdateAppointmentNoteRequest request)
        {
            await _noteService.UpdateNoteAsync(noteId, request);
            return Ok(ApiResponse<object>.SuccessResponse(null!));
        }

        [HttpDelete("{noteId:guid}")]
        public async Task<IActionResult> DeleteNote(Guid noteId)
        {
            await _noteService.DeleteNoteAsync(noteId);
            return Ok(ApiResponse<object>.SuccessResponse(null!));
        }
    }
}
