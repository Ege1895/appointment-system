using AppointmentSystem.Api.Modules.UserModule.DTOs;
using AppointmentSystem.Api.Modules.UserModule.Services;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Requests;
using AppointmentSystem.Api.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AppointmentSystem.Api.Modules.UserModule.Controllers
{
    [ApiController]
    [Route("api/v1/user")]
    public class UserAuthController : ControllerBase
    {
        private readonly IUserAuthService _userAuthService;

        public UserAuthController(IUserAuthService userAuthService)
        {
            _userAuthService = userAuthService;
        }

        [EnableRateLimiting(RateLimitingPolicies.LOGIN_POLICY)]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
        {
            UserAuthResponseDto response = await _userAuthService.RegisterAsync(request);
            return Ok(ApiResponse<UserAuthResponseDto>.SuccessResponse(response));
        }

        [EnableRateLimiting(RateLimitingPolicies.LOGIN_POLICY)]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserRequest request)
        {
            UserAuthResponseDto response = await _userAuthService.LoginAsync(request);
            return Ok(ApiResponse<UserAuthResponseDto>.SuccessResponse(response));
        }

        [EnableRateLimiting(RateLimitingPolicies.REFRESH_TOKEN_POLICY)]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            UserAuthResponseDto response = await _userAuthService.RefreshTokenAsync(request);
            return Ok(ApiResponse<UserAuthResponseDto>.SuccessResponse(response));
        }

        [Authorize(Roles = Roles.User)]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            string? userIdClaim = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(userIdClaim, out Guid userId))
            {
                await _userAuthService.LogoutAsync(userId);
            }
            return Ok(ApiResponse<object>.SuccessResponse(null!));
        }
    }
}
