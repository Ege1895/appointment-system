using AppointmentSystem.Api.Modules.UserModule.DTOs;
using AppointmentSystem.Api.Shared.Constants;
using FluentValidation;

namespace AppointmentSystem.Api.Modules.UserModule.Validators
{
    public class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
    {
        public RegisterUserRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage(ValidationConstants.REQUIRED)
                .MaximumLength(BusinessRules.MAX_NAME_LENGTH).WithMessage(ValidationConstants.MAX_LENGTH);

            RuleFor(x => x.LastName)
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

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage(ValidationConstants.REQUIRED)
                .MaximumLength(BusinessRules.MAX_PHONE_LENGTH).WithMessage(ValidationConstants.MAX_LENGTH);
        }
    }
}
