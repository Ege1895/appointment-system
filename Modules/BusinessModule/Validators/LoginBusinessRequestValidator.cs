using AppointmentSystem.Api.Modules.BusinessModule.DTOs;
using AppointmentSystem.Api.Shared.Constants;
using FluentValidation;

namespace AppointmentSystem.Api.Modules.BusinessModule.Validators
{
    public class LoginBusinessRequestValidator : AbstractValidator<LoginBusinessRequest>
    {
        public LoginBusinessRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(ValidationConstants.REQUIRED)
                .EmailAddress().WithMessage(ValidationConstants.INVALID_EMAIL);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(ValidationConstants.REQUIRED);
        }
    }
}
