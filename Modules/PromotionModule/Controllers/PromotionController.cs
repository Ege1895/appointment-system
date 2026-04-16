using AppointmentSystem.Api.Modules.PromotionModule.DTOs;
using AppointmentSystem.Api.Modules.PromotionModule.Services;
using AppointmentSystem.Api.Shared.Authentication;
using AppointmentSystem.Api.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppointmentSystem.Api.Modules.PromotionModule.Controllers
{
    [ApiController]
    [Route("api/v1/promotions")]
    public class PromotionController : ControllerBase
    {
        private readonly IPromotionService _promotionService;
        private readonly ICurrentUserService _currentUserService;

        public PromotionController(IPromotionService promotionService, ICurrentUserService currentUserService)
        {
            _promotionService = promotionService;
            _currentUserService = currentUserService;
        }

        [HttpGet("packages")]
        public async Task<IActionResult> GetPackages()
        {
            var packages = await _promotionService.GetBoostPackagesAsync();
            return Ok(ApiResponse<List<BoostPackageDto>>.SuccessResponse(packages));
        }

        [HttpPost("boost")]
        [Authorize(Roles = "Business")]
        public async Task<IActionResult> PurchaseBoost([FromBody] PurchaseBoostRequest request)
        {
            if (!_currentUserService.TenantId.HasValue) return Unauthorized();

            var result = await _promotionService.PurchaseBoostAsync(_currentUserService.TenantId.Value, request);
            return Ok(ApiResponse<BusinessBoostResponseDto>.SuccessResponse(result));
        }

        [HttpGet("my-boost")]
        [Authorize(Roles = "Business")]
        public async Task<IActionResult> GetMyBoost()
        {
            if (!_currentUserService.TenantId.HasValue) return Unauthorized();

            var boost = await _promotionService.GetActiveBoostAsync(_currentUserService.TenantId.Value);
            return Ok(ApiResponse<BusinessBoostResponseDto?>.SuccessResponse(boost));
        }
    }
}
