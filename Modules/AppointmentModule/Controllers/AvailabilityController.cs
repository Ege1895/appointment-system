using AppointmentSystem.Api.Modules.AppointmentModule.Services;
using AppointmentSystem.Api.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentSystem.Api.Modules.AppointmentModule.Controllers
{
    [ApiController]
    [Route("api/v1/availability")]
    public class AvailabilityController : ControllerBase
    {
        private readonly IAvailabilityService _availabilityService;

        public AvailabilityController(IAvailabilityService availabilityService)
        {
            _availabilityService = availabilityService;
        }

        [HttpGet("employee/{employeeId:guid}")]
        public async Task<IActionResult> GetAvailableSlots(
            Guid employeeId, 
            [FromQuery] DateTime date, 
            [FromQuery] Guid offeringId,
            [FromQuery] Guid? branchId = null)
        {
            List<TimeSpan> slots = await _availabilityService.GetAvailableSlotsAsync(employeeId, date, offeringId, branchId);
            return Ok(ApiResponse<List<string>>.SuccessResponse(slots.Select(s => s.ToString(@"hh\:mm")).ToList()));
        }
    }
}
