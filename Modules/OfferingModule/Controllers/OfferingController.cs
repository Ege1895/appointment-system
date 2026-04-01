using AppointmentSystem.Api.Modules.OfferingModule.DTOs;
using AppointmentSystem.Api.Modules.OfferingModule.Services;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Responses;

namespace AppointmentSystem.Api.Modules.OfferingModule.Controllers
{
    [ApiController]
    [Route("api/v1/offerings")]
    [Authorize(Roles = Roles.Business)]
    public class OfferingController : ControllerBase
    {
        private readonly IOfferingService _serviceService;

        public OfferingController(IOfferingService serviceService)
        {
            _serviceService = serviceService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOfferingRequest request)
        {
            OfferingResponseDto response = await _serviceService.CreateAsync(request);
            return Ok(ApiResponse<OfferingResponseDto>.SuccessResponse(response));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            (List<OfferingResponseDto> items, PaginationMeta meta) = await _serviceService.GetAllForBusinessAsync(page, pageSize);
            return Ok(ApiResponse<List<OfferingResponseDto>>.SuccessResponse(items, meta));
        }
    }
}
