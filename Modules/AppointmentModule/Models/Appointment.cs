using AppointmentSystem.Api.Modules.AppointmentModule.Enums;
using AppointmentSystem.Api.Modules.BusinessModule.Models;
using AppointmentSystem.Api.Modules.BranchModule.Models;
using AppointmentSystem.Api.Modules.EmployeeModule.Models;
using AppointmentSystem.Api.Modules.OfferingModule.Models;
using AppointmentSystem.Api.Modules.UserModule.Models;
using AppointmentSystem.Api.Shared.Interfaces;
using AppointmentSystem.Api.Shared.Models;

namespace AppointmentSystem.Api.Modules.AppointmentModule.Models
{
    public class Appointment : BaseEntity, IMustHaveTenant
    {
        public Guid BusinessId { get; set; }
        public Business Business { get; set; } = null!;

        public Guid EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid OfferingId { get; set; }
        public Offering Offering { get; set; } = null!;

        public Guid? BranchId { get; set; }
        public virtual Branch? Branch { get; set; }

        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.PendingApproval;

        // Pending Approval / Transfer Timeouts
        public DateTime? ActionTimeoutUtc { get; set; }

        // Delegation (Devretme)
        public Guid? TransferredToEmployeeId { get; set; }
        public Employee? TransferredToEmployee { get; set; }

        // Reschedule (Tarih Değiştirme - keeping old time intact on decline)
        public bool IsRescheduleRequested { get; set; }
        public DateTime? ProposedRescheduleDateUtc { get; set; }
        public DateTime? RescheduleTimeoutUtc { get; set; }

        public DateTime? ReminderSentAt { get; set; }

        public decimal? PriceAtBooking { get; set; }
    }
}
