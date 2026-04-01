using AppointmentSystem.Api.Modules.EmployeeModule.Models;
using AppointmentSystem.Api.Modules.BusinessModule.Models;
using AppointmentSystem.Api.Shared.Interfaces;
using AppointmentSystem.Api.Shared.Models;

namespace AppointmentSystem.Api.Modules.WorkingHourModule.Models
{
    public class EmployeeWorkingHour : BaseEntity, IMustHaveTenant
    {
        public Guid BusinessId { get; set; }
        public Business Business { get; set; } = null!;

        public Guid EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public DayOfWeek DayOfWeek { get; set; }
        public bool IsDayOff { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimeSpan? BreakStartTime { get; set; }
        public TimeSpan? BreakEndTime { get; set; }
    }
}
