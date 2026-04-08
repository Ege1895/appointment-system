using FluentValidation;
using AppointmentSystem.Api.Modules.BusinessModule.DTOs;

namespace AppointmentSystem.Api.Modules.BusinessModule.Validators
{
    public class DeleteBusinessAccountRequestValidator : AbstractValidator<DeleteBusinessAccountRequest>
    {
        public DeleteBusinessAccountRequestValidator()
        {
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifre gereklidir.")
                .NotNull().WithMessage("Şifre gereklidir.");
        }
    }
}
