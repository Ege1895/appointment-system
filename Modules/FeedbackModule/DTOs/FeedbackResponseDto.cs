using AppointmentSystem.Api.Modules.FeedbackModule.Enums;

namespace AppointmentSystem.Api.Modules.FeedbackModule.DTOs
{
    public class FeedbackResponseDto
    {
        public Guid FeedbackId { get; set; }

        public FeedbackCategory Category { get; set; }

        public required string SubmittedByRole { get; set; }

        public Guid? BusinessId { get; set; }

        public Guid? UserId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
