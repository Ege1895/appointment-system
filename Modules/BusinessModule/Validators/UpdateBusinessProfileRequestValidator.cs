using AppointmentSystem.Api.Modules.BusinessModule.DTOs;
using AppointmentSystem.Api.Shared.Constants;
using FluentValidation;

namespace AppointmentSystem.Api.Modules.BusinessModule.Validators
{
    public class UpdateBusinessProfileRequestValidator : AbstractValidator<UpdateBusinessProfileRequest>
    {
        public UpdateBusinessProfileRequestValidator()
        {
            RuleFor(x => x.City)
                .MaximumLength(BusinessRules.MAX_CITY_LENGTH).WithMessage(ValidationConstants.MAX_LENGTH)
                .When(x => x.City != null);

            RuleFor(x => x.District)
                .MaximumLength(BusinessRules.MAX_DISTRICT_LENGTH).WithMessage(ValidationConstants.MAX_LENGTH)
                .When(x => x.District != null);

            RuleFor(x => x.Neighborhood)
                .MaximumLength(BusinessRules.MAX_NEIGHBORHOOD_LENGTH).WithMessage(ValidationConstants.MAX_LENGTH)
                .When(x => x.Neighborhood != null);

            RuleFor(x => x.Description)
                .MaximumLength(BusinessRules.MAX_DESCRIPTION_LENGTH).WithMessage(ValidationConstants.MAX_LENGTH)
                .When(x => x.Description != null);

            RuleFor(x => x.WorkingHourStart)
                .LessThan(x => x.WorkingHourEnd)
                .WithMessage(ValidationConstants.START_BEFORE_END)
                .When(x => x.WorkingHourStart.HasValue && x.WorkingHourEnd.HasValue);
        }
    }
}
