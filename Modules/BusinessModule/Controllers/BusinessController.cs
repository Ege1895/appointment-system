using AppointmentSystem.Api.Modules.BusinessModule.DTOs;
using AppointmentSystem.Api.Modules.BusinessModule.Services;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentSystem.Api.Modules.BusinessModule.Controllers
{
    [ApiController]
    [Route("api/v1/business")]
    [Authorize(Roles = Roles.Business)]
    public class BusinessController : ControllerBase
    {
        private readonly IBusinessService _businessService;

        public BusinessController(IBusinessService businessService)
        {
            _businessService = businessService;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            BusinessResponseDto response = await _businessService.GetProfileAsync();
            
            return Ok(ApiResponse<BusinessResponseDto>.SuccessResponse(response));
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateBusinessProfileRequest request)
        {
            await _businessService.UpdateProfileAsync(request);

            return Ok(ApiResponse<object>.SuccessResponse(null!));
        }
    }
}
