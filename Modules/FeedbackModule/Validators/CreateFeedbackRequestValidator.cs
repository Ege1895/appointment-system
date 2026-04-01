using AppointmentSystem.Api.Modules.FeedbackModule.DTOs;
using AppointmentSystem.Api.Shared.Constants;
using FluentValidation;

namespace AppointmentSystem.Api.Modules.FeedbackModule.Validators
{
    public class CreateFeedbackRequestValidator : AbstractValidator<CreateFeedbackRequest>
    {
        public CreateFeedbackRequestValidator()
        {
            RuleFor(x => x.Text)
                .NotEmpty().WithMessage(ValidationConstants.REQUIRED)
                .MaximumLength(BusinessRules.MAX_FEEDBACK_TEXT_LENGTH).WithMessage(ValidationConstants.MAX_LENGTH);

            RuleFor(x => x.Category)
                .IsInEnum().WithMessage(ValidationConstants.INVALID_GUID);
        }
    }
}
