using AppointmentSystem.Api.Modules.Auth.DTOs;
using AppointmentSystem.Api.Modules.Auth.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentSystem.Api.Modules.Auth.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUnifiedAuthService _unifiedAuthService;

        public AuthController(IUnifiedAuthService unifiedAuthService)
        {
            _unifiedAuthService = unifiedAuthService;
        }

        [HttpPost]
        public async Task<ActionResult<UnifiedAuthResponseDto>> Authenticate([FromBody] UnifiedAuthRequest request)
        {
            UnifiedAuthResponseDto response = await _unifiedAuthService.AuthenticateAsync(request);
            return Ok(response);
        }
    }
}
