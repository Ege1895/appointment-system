using AppointmentSystem.Api.Modules.AppointmentModule.DTOs;
using AppointmentSystem.Api.Modules.AppointmentModule.Services;
using AppointmentSystem.Api.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppointmentSystem.Api.Modules.AppointmentModule.Controllers
{
    [ApiController]
    [Route("api/v1/appointments/{appointmentId:guid}/messages")]
    [Authorize] // All authenticated roles can use messages (subject to service-level check)
    public class AppointmentMessageController : ControllerBase
    {
        private readonly IAppointmentMessageService _messageService;

        public AppointmentMessageController(IAppointmentMessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(Guid appointmentId, [FromBody] SendAppointmentMessageRequest request)
        {
            request.AppointmentId = appointmentId;
            AppointmentMessageResponseDto response = await _messageService.SendAsync(request);
            return Ok(ApiResponse<AppointmentMessageResponseDto>.SuccessResponse(response));
        }

        [HttpGet]
        public async Task<IActionResult> GetThread(Guid appointmentId)
        {
            List<AppointmentMessageResponseDto> response = await _messageService.GetThreadAsync(appointmentId);
            return Ok(ApiResponse<List<AppointmentMessageResponseDto>>.SuccessResponse(response));
        }

        [HttpPost("read")]
        public async Task<IActionResult> MarkRead(Guid appointmentId)
        {
            await _messageService.MarkAsReadAsync(appointmentId);
            return Ok(ApiResponse<object>.SuccessResponse(null!));
        }

        [HttpGet("/api/v1/appointments/messages/unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            int count = await _messageService.GetUnreadCountAsync();
            return Ok(ApiResponse<int>.SuccessResponse(count));
        }
    }
}
