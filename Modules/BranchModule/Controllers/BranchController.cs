using AppointmentSystem.Api.Modules.BranchModule.DTOs;
using AppointmentSystem.Api.Modules.BranchModule.Services;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentSystem.Api.Modules.BranchModule.Controllers
{
    [ApiController]
    [Route("api/v1/branches")]
    [Authorize(Roles = nameof(Roles.Business))]
    public class BranchController : ControllerBase
    {
        private readonly IBranchService _branchService;

        public BranchController(IBranchService branchService)
        {
            _branchService = branchService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBranchRequest request)
        {
            BranchResponseDto response = await _branchService.CreateAsync(request);
            return Ok(ApiResponse<BranchResponseDto>.SuccessResponse(response));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            List<BranchResponseDto> response = await _branchService.GetAllAsync();
            return Ok(ApiResponse<List<BranchResponseDto>>.SuccessResponse(response));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            BranchResponseDto response = await _branchService.GetByIdAsync(id);
            return Ok(ApiResponse<BranchResponseDto>.SuccessResponse(response));
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBranchRequest request)
        {
            await _branchService.UpdateAsync(id, request);
            return Ok(ApiResponse<object>.SuccessResponse(null!));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _branchService.DeleteAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null!));
        }

        [HttpPost("{id:guid}/set-main")]
        public async Task<IActionResult> SetMain(Guid id)
        {
            await _branchService.SetMainBranchAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null!));
        }
    }
}
