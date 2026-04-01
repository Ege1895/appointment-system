using AppointmentSystem.Api.Modules.BusinessModule.DTOs;
using AppointmentSystem.Api.Shared.Constants;
using FluentValidation;

namespace AppointmentSystem.Api.Modules.BusinessModule.Validators
{
    public class RegisterBusinessRequestValidator : AbstractValidator<RegisterBusinessRequest>
    {
        public RegisterBusinessRequestValidator()
        {
            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage(ValidationConstants.REQUIRED)
                .MaximumLength(BusinessRules.MAX_COMPANY_NAME_LENGTH).WithMessage(ValidationConstants.MAX_LENGTH);

            RuleFor(x => x.Subdomain)
                .NotEmpty().WithMessage(ValidationConstants.REQUIRED)
                .MaximumLength(BusinessRules.MAX_SUBDOMAIN_LENGTH).WithMessage(ValidationConstants.MAX_LENGTH)
                .Matches("^[a-z0-9-]+$").WithMessage(ValidationConstants.SUBDOMAIN_FORMAT);

            RuleFor(x => x.OwnerFirstName)
                .NotEmpty().WithMessage(ValidationConstants.REQUIRED)
                .MaximumLength(BusinessRules.MAX_NAME_LENGTH).WithMessage(ValidationConstants.MAX_LENGTH);

            RuleFor(x => x.OwnerLastName)
                .NotEmpty().WithMessage(ValidationConstants.REQUIRED)
                .MaximumLength(BusinessRules.MAX_NAME_LENGTH).WithMessage(ValidationConstants.MAX_LENGTH);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(ValidationConstants.REQUIRED)
                .EmailAddress().WithMessage(ValidationConstants.INVALID_EMAIL)
                .MaximumLength(BusinessRules.MAX_EMAIL_LENGTH).WithMessage(ValidationConstants.MAX_LENGTH);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(ValidationConstants.REQUIRED)
                .MinimumLength(BusinessRules.MIN_PASSWORD_LENGTH).WithMessage(ValidationConstants.MIN_LENGTH)
                .MaximumLength(BusinessRules.MAX_PASSWORD_LENGTH).WithMessage(ValidationConstants.MAX_LENGTH);

            RuleFor(x => x.ContactPhone)
                .NotEmpty().WithMessage(ValidationConstants.REQUIRED)
                .MaximumLength(BusinessRules.MAX_PHONE_LENGTH).WithMessage(ValidationConstants.MAX_LENGTH);
        }
    }
}
