using AppointmentSystem.Api.Modules.BusinessModule.DTOs;
using AppointmentSystem.Api.Modules.BusinessModule.Services;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Requests;
using AppointmentSystem.Api.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AppointmentSystem.Api.Modules.BusinessModule.Controllers
{
    [ApiController]
    [Route("api/v1/businessAuth")]
    public class BusinessAuthController : ControllerBase
    {
        private readonly IBusinessAuthService _businessAuthService;

        public BusinessAuthController(IBusinessAuthService businessAuthService)
        {
            _businessAuthService = businessAuthService;
        }

        [EnableRateLimiting(RateLimitingPolicies.LOGIN_POLICY)]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterBusinessRequest request)
        {
            BusinessAuthResponseDto response = await _businessAuthService.RegisterAsync(request);
            return Ok(ApiResponse<BusinessAuthResponseDto>.SuccessResponse(response));
        }

        [EnableRateLimiting(RateLimitingPolicies.LOGIN_POLICY)]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginBusinessRequest request)
        {
            BusinessAuthResponseDto response = await _businessAuthService.LoginAsync(request);
            return Ok(ApiResponse<BusinessAuthResponseDto>.SuccessResponse(response));
        }

        [EnableRateLimiting(RateLimitingPolicies.REFRESH_TOKEN_POLICY)]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            BusinessAuthResponseDto response = await _businessAuthService.RefreshTokenAsync(request);
            return Ok(ApiResponse<BusinessAuthResponseDto>.SuccessResponse(response));
        }

        [Authorize(Roles = Roles.Business)]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            string? businessIdClaim = HttpContext.User.FindFirst("BusinessId")?.Value
                ?? HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(businessIdClaim, out Guid businessId))
            {
                await _businessAuthService.LogoutAsync(businessId);
            }
            return Ok(ApiResponse<object>.SuccessResponse(null!));
        }
    }
}
