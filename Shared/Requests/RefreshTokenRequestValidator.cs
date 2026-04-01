using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Requests;
using FluentValidation;

namespace AppointmentSystem.Api.Shared.Validators
{
    public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
    {
        public RefreshTokenRequestValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage(ValidationConstants.REQUIRED);
        }
    }
}
