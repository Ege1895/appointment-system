using AppointmentSystem.Api.Modules.OfferingModule.DTOs;
using AppointmentSystem.Api.Shared.Constants;
using FluentValidation;

namespace AppointmentSystem.Api.Modules.OfferingModule.Validators
{
    public class CreateOfferingRequestValidator : AbstractValidator<CreateOfferingRequest>
    {
        public CreateOfferingRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(ValidationConstants.REQUIRED)
                .MaximumLength(BusinessRules.MAX_SERVICE_NAME_LENGTH).WithMessage(ValidationConstants.MAX_LENGTH);

            RuleFor(x => x.Description)
                .MaximumLength(BusinessRules.MAX_SERVICE_DESC_LENGTH).WithMessage(ValidationConstants.MAX_LENGTH)
                .When(x => x.Description != null);

            RuleFor(x => x.DurationInMinutes)
                .GreaterThan(0).WithMessage(ValidationConstants.MUST_BE_POSITIVE);

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage(ValidationConstants.MUST_BE_NON_NEGATIVE)
                .When(x => x.Price.HasValue);
        }
    }
}
