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
    }
}
