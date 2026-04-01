using AppointmentSystem.Api.Modules.AppointmentModule.DTOs;
using AppointmentSystem.Api.Modules.AppointmentModule.Services;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentSystem.Api.Modules.AppointmentModule.Controllers
{
    [ApiController]
    [Route("api/v1/appointments")]
    [Authorize]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpPost]
        [Authorize(Roles = Roles.User)]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request)
        {
            AppointmentResponseDto response = await _appointmentService.CreateAsync(request);
            return Ok(ApiResponse<AppointmentResponseDto>.SuccessResponse(response));
        }

        [HttpGet("me")]
        [Authorize(Roles = Roles.User)]
        public async Task<IActionResult> GetMyAppointments()
        {
            List<AppointmentResponseDto> response = await _appointmentService.GetMyAsync();
            return Ok(ApiResponse<List<AppointmentResponseDto>>.SuccessResponse(response));
        }

        [HttpGet("me/upcoming")]
        [Authorize(Roles = Roles.User)]
        public async Task<IActionResult> GetMyUpcomingAppointments([FromQuery] int limit = 5)
        {
            List<AppointmentResponseDto> response = await _appointmentService.GetMyUpcomingAsync(limit);
            return Ok(ApiResponse<List<AppointmentResponseDto>>.SuccessResponse(response));
        }

        [HttpGet("business")]
        [Authorize(Roles = Roles.Business)]
        public async Task<IActionResult> GetBusinessAppointments([FromQuery] DateTime? date)
        {
            List<AppointmentResponseDto> response = await _appointmentService.GetBusinessAppointmentsAsync(date);
            return Ok(ApiResponse<List<AppointmentResponseDto>>.SuccessResponse(response));
        }

        // Cancel: both Business owner and the booking customer may cancel
        [HttpDelete("{id}")]
        [Authorize(Roles = $"{Roles.Business},{Roles.User}")]
        public async Task<IActionResult> CancelAppointment(Guid id)
        {
            await _appointmentService.CancelAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null!));
        }

        // Accept/Decline/Transfer: Business owner or assigned employee (User-role with employee link)
        [HttpPost("{id}/accept")]
        [Authorize(Roles = $"{Roles.Business},{Roles.User}")]
        public async Task<IActionResult> Accept(Guid id)
        {
            await _appointmentService.AcceptAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null!));
        }

        [HttpPost("{id}/decline")]
        [Authorize(Roles = $"{Roles.Business},{Roles.User}")]
        public async Task<IActionResult> Decline(Guid id)
        {
            await _appointmentService.DeclineAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null!));
        }

        [HttpPost("{id}/transfer")]
        [Authorize(Roles = $"{Roles.Business},{Roles.User}")]
        public async Task<IActionResult> Transfer(Guid id, [FromBody] Guid newEmployeeId)
        {
            await _appointmentService.TransferAsync(id, newEmployeeId);
            return Ok(ApiResponse<object>.SuccessResponse(null!));
        }

        // RespondToTransfer: the receiving employee (User role) or Business owner responds
        [HttpPost("{id}/transfer/respond")]
        [Authorize(Roles = $"{Roles.Business},{Roles.User}")]
        public async Task<IActionResult> RespondToTransfer(Guid id, [FromQuery] bool isAccepted)
        {
            await _appointmentService.RespondToTransferAsync(id, isAccepted);
            return Ok(ApiResponse<object>.SuccessResponse(null!));
        }

        // RequestReschedule: both Business owner and customer may propose
        [HttpPost("{id}/reschedule")]
        [Authorize(Roles = $"{Roles.Business},{Roles.User}")]
        public async Task<IActionResult> RequestReschedule(Guid id, [FromBody] DateTime newStartTimeUtc)
        {
            await _appointmentService.RequestRescheduleAsync(id, newStartTimeUtc);
            return Ok(ApiResponse<object>.SuccessResponse(null!));
        }

        // RespondToReschedule: the other party (Business owner or assigned employee) responds
        [HttpPost("{id}/reschedule/respond")]
        [Authorize(Roles = $"{Roles.Business},{Roles.User}")]
        public async Task<IActionResult> RespondToReschedule(Guid id, [FromQuery] bool isAccepted)
        {
            await _appointmentService.RespondToRescheduleAsync(id, isAccepted);
            return Ok(ApiResponse<object>.SuccessResponse(null!));
        }
    }
}
