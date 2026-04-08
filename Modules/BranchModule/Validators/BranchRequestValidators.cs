using AppointmentSystem.Api.Modules.BranchModule.DTOs;
using AppointmentSystem.Api.Shared.Constants;
using FluentValidation;

namespace AppointmentSystem.Api.Modules.BranchModule.Validators
{
    public class CreateBranchRequestValidator : AbstractValidator<CreateBranchRequest>
    {
        public CreateBranchRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(ValidationConstants.REQUIRED)
                .MaximumLength(BusinessRules.MAX_BRANCH_NAME_LENGTH).WithMessage(ValidationConstants.MAX_LENGTH);

            RuleFor(x => x.Address)
                .MaximumLength(BusinessRules.MAX_BRANCH_ADDRESS_LENGTH).WithMessage(ValidationConstants.MAX_LENGTH);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(BusinessRules.MAX_PHONE_LENGTH).WithMessage(ValidationConstants.MAX_LENGTH);

            RuleFor(x => x.City)
                .MaximumLength(BusinessRules.MAX_CITY_LENGTH).WithMessage(ValidationConstants.MAX_LENGTH);

            RuleFor(x => x.District)
                .MaximumLength(BusinessRules.MAX_DISTRICT_LENGTH).WithMessage(ValidationConstants.MAX_LENGTH);
        }
    }

    public class UpdateBranchRequestValidator : AbstractValidator<UpdateBranchRequest>
    {
        public UpdateBranchRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(ValidationConstants.REQUIRED)
                .MaximumLength(BusinessRules.MAX_BRANCH_NAME_LENGTH).WithMessage(ValidationConstants.MAX_LENGTH);

            RuleFor(x => x.Address)
                .MaximumLength(BusinessRules.MAX_BRANCH_ADDRESS_LENGTH).WithMessage(ValidationConstants.MAX_LENGTH);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(BusinessRules.MAX_PHONE_LENGTH).WithMessage(ValidationConstants.MAX_LENGTH);

            RuleFor(x => x.City)
                .MaximumLength(BusinessRules.MAX_CITY_LENGTH).WithMessage(ValidationConstants.MAX_LENGTH);

            RuleFor(x => x.District)
                .MaximumLength(BusinessRules.MAX_DISTRICT_LENGTH).WithMessage(ValidationConstants.MAX_LENGTH);
        }
    }
}
