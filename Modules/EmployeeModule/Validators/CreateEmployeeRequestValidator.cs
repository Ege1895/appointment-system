using AppointmentSystem.Api.Modules.EmployeeModule.DTOs;
using AppointmentSystem.Api.Shared.Constants;
using FluentValidation;

namespace AppointmentSystem.Api.Modules.EmployeeModule.Validators
{
    public class CreateEmployeeRequestValidator : AbstractValidator<CreateEmployeeRequest>
    {
        public CreateEmployeeRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(ValidationConstants.REQUIRED)
                .EmailAddress().WithMessage(ValidationConstants.INVALID_EMAIL)
                .MaximumLength(BusinessRules.MAX_EMAIL_LENGTH).WithMessage(ValidationConstants.MAX_LENGTH);

            RuleFor(x => x.JobTitle)
                .NotEmpty().WithMessage(ValidationConstants.REQUIRED)
                .MaximumLength(BusinessRules.MAX_JOB_TITLE_LENGTH).WithMessage(ValidationConstants.MAX_LENGTH);

            RuleFor(x => x.JobDescription)
                .NotEmpty().WithMessage(ValidationConstants.REQUIRED)
                .MaximumLength(BusinessRules.MAX_JOB_DESCRIPTION_LENGTH).WithMessage(ValidationConstants.MAX_LENGTH);
        }
    }
}
