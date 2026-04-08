using AppointmentSystem.Api.Modules.AppointmentModule.Enums;

namespace AppointmentSystem.Api.Modules.AppointmentModule.DTOs
{
    public class AppointmentResponseDto
    {
        public Guid AppointmentId { get; set; }
        public Guid BusinessId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        
        public Guid UserId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        
        public Guid OfferingId { get; set; }
        public string OfferingName { get; set; } = string.Empty;
        
        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }

        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        
        public AppointmentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // State Machine Properties
        public DateTime? ActionTimeoutUtc { get; set; }
        public Guid? TransferredToEmployeeId { get; set; }
        public string? TransferredToEmployeeName { get; set; }
        public bool IsRescheduleRequested { get; set; }
        public DateTime? ProposedRescheduleDateUtc { get; set; }
        public DateTime? RescheduleTimeoutUtc { get; set; }
    }
}
