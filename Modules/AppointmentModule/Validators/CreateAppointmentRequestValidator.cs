using AppointmentSystem.Api.Modules.AppointmentModule.DTOs;
using AppointmentSystem.Api.Shared.Constants;
using FluentValidation;

namespace AppointmentSystem.Api.Modules.AppointmentModule.Validators
{
    public class CreateAppointmentRequestValidator : AbstractValidator<CreateAppointmentRequest>
    {
        public CreateAppointmentRequestValidator()
        {
            RuleFor(x => x.BusinessId)
                .NotEmpty().WithMessage(ValidationConstants.INVALID_GUID);

            RuleFor(x => x.EmployeeId)
                .NotEmpty().WithMessage(ValidationConstants.INVALID_GUID);

            RuleFor(x => x.OfferingId)
                .NotEmpty().WithMessage(ValidationConstants.INVALID_GUID);

            RuleFor(x => x.StartTimeUtc)
                .NotEmpty().WithMessage(ValidationConstants.REQUIRED)
                .GreaterThan(DateTime.UtcNow).WithMessage(ValidationConstants.MUST_BE_FUTURE);
        }
    }
}
