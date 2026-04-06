using AppointmentSystem.Api.Modules.EmployeeModule.DTOs;
using FluentValidation;

namespace AppointmentSystem.Api.Modules.EmployeeModule.Validators
{
    public class AssignEmployeeToBranchRequestValidator : AbstractValidator<AssignEmployeeToBranchRequest>
    {
        public AssignEmployeeToBranchRequestValidator()
        {
            // BranchId nullable olabilir (atama kaldırma için)
            // Özel bir kısıtlama gerekmiyor ancak yapısal olarak validator bulunması iyidir.
        }
    }
}
