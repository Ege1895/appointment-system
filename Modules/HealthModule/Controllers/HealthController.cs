using Microsoft.AspNetCore.Mvc;
using AppointmentSystem.Api.Modules.HealthModule.Services;
using AppointmentSystem.Api.Modules.HealthModule.Models;
using AppointmentSystem.Api.Shared.Responses;

namespace AppointmentSystem.Api.Modules.HealthModule.Controllers
{
    [ApiController]
    [Route("api/health")]
    public class HealthController : ControllerBase
    {
        private readonly IHealthService _healthService;

        public HealthController(IHealthService healthService)
        {
            _healthService = healthService;
        }

        [HttpGet]
        public ActionResult<ApiResponse<HealthResponse>> Get()
        {
            HealthResponse data = _healthService.GetHealth();

            ApiResponse<HealthResponse> response = ApiResponse<HealthResponse>.SuccessResponse(data);

            return Ok(response);
        }

    }
}