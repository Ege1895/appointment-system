using AppointmentSystem.Api.Modules.BusinessModule.DTOs;
using AppointmentSystem.Api.Modules.BusinessModule.Services;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AppointmentSystem.Api.Modules.BusinessModule.Controllers
{
    [ApiController]
    [Route("api/v1/business/import")]
    [Authorize(Roles = nameof(Roles.Business))]
    [EnableRateLimiting(RateLimitingPolicies.IMPORT_POLICY)]
    public class BusinessImportController : ControllerBase
    {
        private readonly IImportService _importService;

        public BusinessImportController(IImportService importService)
        {
            _importService = importService;
        }

        [HttpPost("employees")]
        [ProducesResponseType(typeof(ApiResponse<ImportResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ImportEmployees([FromForm] IFormFile file)
        {
            ValidateFile(file);
            ImportResultDto result = await _importService.ImportEmployeesAsync(file);
            return Ok(ApiResponse<ImportResultDto>.SuccessResponse(result));
        }

        [HttpPost("customers")]
        [ProducesResponseType(typeof(ApiResponse<ImportResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ImportCustomers([FromForm] IFormFile file)
        {
            ValidateFile(file);
            ImportResultDto result = await _importService.ImportCustomersAsync(file);
            return Ok(ApiResponse<ImportResultDto>.SuccessResponse(result));
        }

        private void ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new FluentValidation.ValidationException(ErrorMessages.IMPORT_FILE_REQUIRED);
            }

            // Boyut kontrolü (MB -> Bytes)
            long maxSizeBytes = BusinessRules.MAX_IMPORT_FILE_SIZE_MB * 1024 * 1024;
            if (file.Length > maxSizeBytes)
            {
                throw new FluentValidation.ValidationException(ErrorMessages.IMPORT_FILE_TOO_LARGE);
            }

            // Format kontrolü
            string extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".csv" && extension != ".xlsx")
            {
                throw new FluentValidation.ValidationException(ErrorMessages.IMPORT_INVALID_FORMAT);
            }
        }
    }
}
