using AppointmentSystem.Api.Modules.UserModule.DTOs;
using AppointmentSystem.Api.Shared.Constants;
using FluentValidation;

namespace AppointmentSystem.Api.Modules.UserModule.Validators
{
    public class LoginUserRequestValidator : AbstractValidator<LoginUserRequest>
    {
        public LoginUserRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(ValidationConstants.REQUIRED)
                .EmailAddress().WithMessage(ValidationConstants.INVALID_EMAIL);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(ValidationConstants.REQUIRED);
        }
    }
}
