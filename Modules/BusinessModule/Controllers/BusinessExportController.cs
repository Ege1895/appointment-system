using AppointmentSystem.Api.Modules.BusinessModule.DTOs;
using AppointmentSystem.Api.Modules.BusinessModule.Services;
using AppointmentSystem.Api.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AppointmentSystem.Api.Modules.BusinessModule.Controllers
{
    [ApiController]
    [Route("api/v1/business/export")]
    [Authorize(Roles = nameof(Roles.Business))]
    [EnableRateLimiting(RateLimitingPolicies.EXPORT_POLICY)]
    public class BusinessExportController : ControllerBase
    {
        private readonly IExportService _exportService;

        public BusinessExportController(IExportService exportService)
        {
            _exportService = exportService;
        }

        [HttpPost("appointments")]
        public async Task<IActionResult> ExportAppointments([FromBody] ExportAppointmentsRequest request)
        {
            var (content, fileName, contentType) = await _exportService.ExportAppointmentsAsync(request);
            
            return File(content, contentType, fileName);
        }

        [HttpPost("customers")]
        public async Task<IActionResult> ExportCustomers([FromBody] ExportCustomersRequest request)
        {
            var (content, fileName, contentType) = await _exportService.ExportCustomersAsync(request);
            
            return File(content, contentType, fileName);
        }
    }
}
