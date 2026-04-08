using FluentValidation;
using AppointmentSystem.Api.Modules.AppointmentModule.DTOs;
using AppointmentSystem.Api.Shared.Constants;

namespace AppointmentSystem.Api.Modules.AppointmentModule.Validators
{
    public class CreateAppointmentNoteRequestValidator : AbstractValidator<CreateAppointmentNoteRequest>
    {
        public CreateAppointmentNoteRequestValidator()
        {
            RuleFor(x => x.AppointmentId)
                .NotEmpty().WithMessage(ValidationConstants.REQUIRED);

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage(ValidationConstants.REQUIRED)
                .MaximumLength(BusinessRules.MAX_NOTE_CONTENT_LENGTH)
                .WithMessage($"{ValidationConstants.MAX_LENGTH} {BusinessRules.MAX_NOTE_CONTENT_LENGTH}");
        }
    }
}
