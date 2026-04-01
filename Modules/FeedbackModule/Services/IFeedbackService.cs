using AppointmentSystem.Api.Modules.FeedbackModule.DTOs;

namespace AppointmentSystem.Api.Modules.FeedbackModule.Services
{
    public interface IFeedbackService
    {
        Task<FeedbackResponseDto> CreateAsync(CreateFeedbackRequest request);
    }
}
