using AppointmentSystem.Api.Modules.SubscriptionModule.DTOs;
using AppointmentSystem.Api.Modules.SubscriptionModule.Services;
using AppointmentSystem.Api.Shared.Authentication;
using AppointmentSystem.Api.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentSystem.Api.Modules.SubscriptionModule.Controllers
{
    [ApiController]
    [Route("api/v1/subscription")]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly ICurrentUserService _currentUserService;

        public SubscriptionController(ISubscriptionService subscriptionService, ICurrentUserService currentUserService)
        {
            _subscriptionService = subscriptionService;
            _currentUserService = currentUserService;
        }

        [HttpGet("packages")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPackages()
        {
            var packages = await _subscriptionService.GetAvailablePackagesAsync();
            return Ok(ApiResponse<List<SubscriptionPackageDto>>.SuccessResponse(packages));
        }

        [HttpGet("my")]
        [Authorize(Roles = "Business")]
        public async Task<IActionResult> GetMySubscription()
        {
            if (!_currentUserService.TenantId.HasValue) return Unauthorized();
            
            var sub = await _subscriptionService.GetMySubscriptionAsync(_currentUserService.TenantId.Value);
            if (sub == null) return NotFound("Subscription not found.");
            
            return Ok(ApiResponse<BusinessSubscriptionDto>.SuccessResponse(sub));
        }

        [HttpPost("upgrade")]
        [Authorize(Roles = "Business")]
        public async Task<IActionResult> Upgrade([FromBody] UpgradePackageRequest request)
        {
            if (!_currentUserService.TenantId.HasValue) return Unauthorized();
            
            var result = await _subscriptionService.RequestUpgradeAsync(_currentUserService.TenantId.Value, request);
            return Ok(ApiResponse<UpgradePackageResponseDto>.SuccessResponse(result));
        }

        [HttpPost("upgrade/{id}/confirm")]
        [Authorize(Roles = "Business")]
        public async Task<IActionResult> ConfirmUpgrade(Guid id)
        {
            var result = await _subscriptionService.ConfirmUpgradeAsync(id);
            return Ok(ApiResponse<UpgradePackageResponseDto>.SuccessResponse(result));
        }

        [HttpDelete("upgrade/{id}")]
        [Authorize(Roles = "Business")]
        public async Task<IActionResult> CancelUpgrade(Guid id)
        {
            await _subscriptionService.CancelUpgradeRequestAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null));
        }
    }
}
