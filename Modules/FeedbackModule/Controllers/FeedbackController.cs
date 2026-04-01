using AppointmentSystem.Api.Modules.FeedbackModule.DTOs;
using AppointmentSystem.Api.Modules.FeedbackModule.Services;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentSystem.Api.Modules.FeedbackModule.Controllers
{
    [ApiController]
    [Route("api/v1/feedback")]
    [Authorize(Roles = $"{Roles.Business},{Roles.User}")]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFeedbackRequest request)
        {
            FeedbackResponseDto result = await _feedbackService.CreateAsync(request);
            return Ok(ApiResponse<FeedbackResponseDto>.SuccessResponse(result));
        }
    }
}
