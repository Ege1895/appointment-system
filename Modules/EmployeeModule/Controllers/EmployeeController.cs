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
            return Ok(response);
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> CreateEmployees([FromBody] IEnumerable<CreateEmployeeRequest> requests)
        {
            // Handles multiple employee creations efficiently using EF Core AddRangeAsync
            IEnumerable<EmployeeResponseDto> response = await _employeeService.CreateEmployeesAsync(requests);
            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetEmployeesQuery query)
        {
            int totalCount = await _employeeService.GetCountAsync();

            IEnumerable<EmployeeResponseDto> employees = await _employeeService.GetAllAsync(query.Page, query.PageSize);

            PaginationMeta meta = PaginationMeta.Create(query.Page, query.PageSize, totalCount);

            return Ok(ApiResponse<IEnumerable<EmployeeResponseDto>>.SuccessResponse(employees, meta));
        }
    }
}
