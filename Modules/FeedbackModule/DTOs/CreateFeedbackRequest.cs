using AppointmentSystem.Api.Modules.FeedbackModule.Enums;

namespace AppointmentSystem.Api.Modules.FeedbackModule.DTOs
{
    public class CreateFeedbackRequest
    {
        public FeedbackCategory Category { get; set; }

        public required string Text { get; set; }
    }
}
