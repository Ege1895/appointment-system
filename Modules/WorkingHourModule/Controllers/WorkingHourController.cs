using AppointmentSystem.Api.Modules.WorkingHourModule.DTOs;
using AppointmentSystem.Api.Modules.WorkingHourModule.Services;
using AppointmentSystem.Api.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppointmentSystem.Api.Shared.Constants;

namespace AppointmentSystem.Api.Modules.WorkingHourModule.Controllers
{
    [ApiController]
    [Route("api/v1/employees/{employeeId}/working-hours")]
    public class WorkingHourController : ControllerBase
    {
        private readonly IWorkingHourService _workingHourService;

        public WorkingHourController(IWorkingHourService workingHourService)
        {
            _workingHourService = workingHourService;
        }

        [HttpPost]
        [Authorize(Roles = nameof(Roles.Business) + "," + nameof(Roles.User))]
        public async Task<IActionResult> SetWorkingHours(Guid employeeId, [FromBody] List<WorkingHourDto> request)
        {
            await _workingHourService.SetWorkingHoursAsync(employeeId, request);
            return Ok(ApiResponse<object>.SuccessResponse(null!));
        }

        [HttpGet]
        [Authorize(Roles = nameof(Roles.Business) + "," + nameof(Roles.User))]
        public async Task<IActionResult> GetWorkingHours(Guid employeeId)
        {
            List<WorkingHourDto> response = await _workingHourService.GetWorkingHoursAsync(employeeId);
            return Ok(ApiResponse<List<WorkingHourDto>>.SuccessResponse(response));
        }
    }
}
