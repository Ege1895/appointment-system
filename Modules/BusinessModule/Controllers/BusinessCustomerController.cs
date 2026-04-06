using System;
using System.Net;
using System.Threading.Tasks;
using AppointmentSystem.Api.Modules.BusinessModule.DTOs;
using AppointmentSystem.Api.Modules.BusinessModule.Services;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentSystem.Api.Modules.BusinessModule.Controllers
{
    [ApiController]
    [Route("api/v1/business/customers")]
    [Authorize(Roles = nameof(Roles.Business))]
    public class BusinessCustomerController : ControllerBase
    {
        private readonly IBusinessCustomerService _businessCustomerService;

        public BusinessCustomerController(IBusinessCustomerService businessCustomerService)
        {
            _businessCustomerService = businessCustomerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<BusinessCustomerSummaryDto>>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCustomers([FromQuery] GetCustomersQuery query)
        {
            var (items, meta) = await _businessCustomerService.GetCustomersAsync(query);
            return Ok(ApiResponse<IReadOnlyList<BusinessCustomerSummaryDto>>.SuccessResponse(items, meta));
        }

        [HttpGet("{customerId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<BusinessCustomerDetailDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetCustomerDetail(Guid customerId)
        {
            try
            {
                var detail = await _businessCustomerService.GetCustomerDetailAsync(customerId);
                return Ok(ApiResponse<BusinessCustomerDetailDto>.SuccessResponse(detail));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
            }
        }

        [HttpPost("block")]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> BlockCustomer([FromBody] BlockCustomerRequest request)
        {
            try
            {
                await _businessCustomerService.BlockCustomerAsync(request);
                return Ok(ApiResponse<object>.SuccessResponse(null!));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
            }
        }

        [HttpDelete("{userId:guid}/block")]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> UnblockCustomer(Guid userId)
        {
            try
            {
                await _businessCustomerService.UnblockCustomerAsync(userId);
                return Ok(ApiResponse<object>.SuccessResponse(null!));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
            }
        }

        [HttpGet("blocked")]
        [ProducesResponseType(typeof(ApiResponse<List<BlockedCustomerResponseDto>>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetBlockedCustomers()
        {
            var blockedCustomers = await _businessCustomerService.GetBlockedCustomersAsync();
            return Ok(ApiResponse<List<BlockedCustomerResponseDto>>.SuccessResponse(blockedCustomers));
        }

        [HttpPost("{customerId:guid}/email")]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> SendEmailToCustomer(Guid customerId, [FromBody] SendCustomerEmailRequest request)
        {
            if (customerId != request.CustomerId)
            {
                return BadRequest(ApiResponse<object>.FailureResponse("Müşteri ID uyuşmazlığı."));
            }

            try
            {
                await _businessCustomerService.SendEmailToCustomerAsync(request);
                return Ok(ApiResponse<object>.SuccessResponse(null!));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
            }
        }
    }
}
