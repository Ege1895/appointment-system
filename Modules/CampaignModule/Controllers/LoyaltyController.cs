using AppointmentSystem.Api.Modules.CampaignModule.DTOs;
using AppointmentSystem.Api.Modules.CampaignModule.Services;
using AppointmentSystem.Api.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentSystem.Api.Modules.CampaignModule.Controllers
{
    [ApiController]
    [Route("api/v1/loyalty")]
    public class LoyaltyController : ControllerBase
    {
        private readonly ILoyaltyService _loyaltyService;

        public LoyaltyController(ILoyaltyService loyaltyService)
        {
            _loyaltyService = loyaltyService;
        }

        [HttpPost("program")]
        [Authorize(Roles = "Business")]
        public async Task<IActionResult> SetupProgram([FromBody] SetupLoyaltyProgramRequest request)
        {
            LoyaltyProgramResponseDto result = await _loyaltyService.SetupProgramAsync(request);
            return Ok(ApiResponse<LoyaltyProgramResponseDto>.SuccessResponse(result));
        }

        [HttpGet("program/{businessId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProgram(Guid businessId)
        {
            LoyaltyProgramResponseDto? result = await _loyaltyService.GetProgramAsync(businessId);
            if (result == null) return NotFound();
            return Ok(ApiResponse<LoyaltyProgramResponseDto>.SuccessResponse(result));
        }

        [HttpGet("points/{businessId:guid}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetUserPoints(Guid businessId)
        {
            UserPointsResponseDto result = await _loyaltyService.GetUserPointsAsync(businessId);
            return Ok(ApiResponse<UserPointsResponseDto>.SuccessResponse(result));
        }

        [HttpGet("history/{businessId:guid}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetTransactionHistory(Guid businessId)
        {
            List<LoyaltyTransactionResponseDto> result = await _loyaltyService.GetTransactionHistoryAsync(businessId);
            return Ok(ApiResponse<List<LoyaltyTransactionResponseDto>>.SuccessResponse(result));
        }
    }
}
