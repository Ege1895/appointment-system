using System.Collections.Generic;
using System.Threading.Tasks;
using AppointmentSystem.Api.Modules.EmployeeModule.DTOs;
using AppointmentSystem.Api.Modules.EmployeeModule.Services;
using AppointmentSystem.Api.Shared.Responses;
using Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppointmentSystem.Api.Shared.Constants;

namespace AppointmentSystem.Api.Modules.EmployeeModule.Controllers
{
    [ApiController]
    [Route("api/v1/employees")]
    [Authorize(Roles = nameof(Roles.Business))]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequest request)
        {
            EmployeeResponseDto response = await _employeeService.CreateEmployeeAsync(request);
            return Ok(ApiResponse<EmployeeResponseDto>.SuccessResponse(response));
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> CreateEmployees([FromBody] IEnumerable<CreateEmployeeRequest> requests)
        {
            // Handles multiple employee creations efficiently using EF Core AddRangeAsync
            IEnumerable<EmployeeResponseDto> response = await _employeeService.CreateEmployeesAsync(requests);
            return Ok(ApiResponse<IEnumerable<EmployeeResponseDto>>.SuccessResponse(response));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetEmployeesQuery query)
        {
            int totalCount = await _employeeService.GetCountAsync();

            IEnumerable<EmployeeResponseDto> employees = await _employeeService.GetAllAsync(query.Page, query.PageSize);

            PaginationMeta meta = PaginationMeta.Create(query.Page, query.PageSize, totalCount);

            return Ok(ApiResponse<IEnumerable<EmployeeResponseDto>>.SuccessResponse(employees, meta));
        }

        [HttpPost("{id:guid}/branch")]
        public async Task<IActionResult> AssignBranch(Guid id, [FromBody] AssignEmployeeToBranchRequest request)
        {
            await _employeeService.AssignBranchAsync(id, request.BranchId);
            return Ok(ApiResponse<object>.SuccessResponse(null!));
        }

        [HttpGet("branch/{branchId:guid}")]
        public async Task<IActionResult> GetByBranch(Guid branchId)
        {
            IEnumerable<EmployeeResponseDto> employees = await _employeeService.GetByBranchAsync(branchId);
            return Ok(ApiResponse<IEnumerable<EmployeeResponseDto>>.SuccessResponse(employees));
        }

        [HttpPut("{id:guid}/offerings")]
        public async Task<IActionResult> SetOfferings(Guid id, [FromBody] AssignOfferingsRequest request)
        {
            await _employeeService.SetOfferingsAsync(id, request.OfferingIds);
            return Ok(ApiResponse<object>.SuccessResponse(null!));
        }

        [HttpGet("{id:guid}/offerings")]
        public async Task<IActionResult> GetOfferings(Guid id)
        {
            List<EmployeeOfferingResponseDto> response = await _employeeService.GetOfferingsAsync(id);
            return Ok(ApiResponse<List<EmployeeOfferingResponseDto>>.SuccessResponse(response));
        }

        [HttpGet("/api/v1/businesses/{businessId:guid}/offerings/{offeringId:guid}/employees")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicEmployeesByOffering(Guid businessId, Guid offeringId)
        {
            IEnumerable<EmployeeResponseDto> response = await _employeeService.GetByOfferingAsync(businessId, offeringId);
            return Ok(ApiResponse<IEnumerable<EmployeeResponseDto>>.SuccessResponse(response));
        }
    }
}
