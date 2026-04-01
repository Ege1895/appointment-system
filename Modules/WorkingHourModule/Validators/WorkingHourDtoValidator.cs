using AppointmentSystem.Api.Modules.WorkingHourModule.DTOs;
using AppointmentSystem.Api.Shared.Constants;
using FluentValidation;

namespace AppointmentSystem.Api.Modules.WorkingHourModule.Validators
{
    public class WorkingHourDtoValidator : AbstractValidator<WorkingHourDto>
    {
        public WorkingHourDtoValidator()
        {
            RuleFor(x => x.StartTime)
                .LessThan(x => x.EndTime)
                .WithMessage(ValidationConstants.START_BEFORE_END)
                .Unless(x => x.IsDayOff);

            RuleFor(x => x.BreakStartTime)
                .LessThan(x => x.BreakEndTime)
                .WithMessage(ValidationConstants.BREAK_START_BEFORE_BREAK_END)
                .When(x => x.BreakStartTime.HasValue && x.BreakEndTime.HasValue);
        }
    }
}
