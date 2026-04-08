using FluentValidation;
using AppointmentSystem.Api.Modules.BusinessModule.DTOs;

namespace AppointmentSystem.Api.Modules.BusinessModule.Validators
{
    public class ExportAppointmentsRequestValidator : AbstractValidator<ExportAppointmentsRequest>
    {
        public ExportAppointmentsRequestValidator()
        {
            RuleFor(x => x.StartDate)
                .LessThanOrEqualTo(x => x.EndDate ?? DateTime.UtcNow)
                .When(x => x.StartDate.HasValue)
                .WithMessage("Başlangıç tarihi bitiş tarihinden sonra olamaz.");

            RuleFor(x => x)
                .Must(x => {
                    DateTime end = x.EndDate ?? DateTime.UtcNow;
                    DateTime start = x.StartDate ?? end.AddDays(-90);
                    return (end - start).TotalDays <= 365 * 2;
                })
                .WithMessage("Dışa aktarma tarih aralığı maksimum 2 yıl olabilir.");
        }
    }
}
