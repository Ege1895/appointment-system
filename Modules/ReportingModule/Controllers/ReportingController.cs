using AppointmentSystem.Api.Modules.ReportingModule.DTOs;
using AppointmentSystem.Api.Modules.ReportingModule.Services;
using AppointmentSystem.Api.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppointmentSystem.Api.Shared.Responses;
using System;
using System.Net;
using System.Threading.Tasks;

namespace AppointmentSystem.Api.Modules.ReportingModule.Controllers
{
    [ApiController]
    [Route("api/v1/reporting")]
    [Authorize(Roles = nameof(Roles.Business))]
    public class ReportingController : ControllerBase
    {
        private readonly IReportingService _reportingService;

        public ReportingController(IReportingService reportingService)
        {
            _reportingService = reportingService;
        }

        [HttpGet("appointments/count")]
        [ProducesResponseType(typeof(ApiResponse<AppointmentCountReportResponseDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAppointmentCountReport([FromQuery] AppointmentCountReportRequest request)
        {
            try
            {
                var report = await _reportingService.GetAppointmentCountReportAsync(request);
                return Ok(ApiResponse<AppointmentCountReportResponseDto>.SuccessResponse(report));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, 
                    ApiResponse<object>.FailureResponse("Rapor oluşturulurken bir hata oluştu: " + ex.Message));
            }
        }
    }
}
