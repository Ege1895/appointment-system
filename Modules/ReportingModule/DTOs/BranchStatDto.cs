using System;

namespace AppointmentSystem.Api.Modules.ReportingModule.DTOs
{
    public class BranchStatDto
    {
        public Guid? BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public bool IsMain { get; set; }
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public double CompletionRate => TotalAppointments == 0 ? 0 : (double)CompletedAppointments / TotalAppointments * 100;
        public decimal TotalRevenue { get; set; }
        public int ActiveEmployees { get; set; }
        public string? BusiestDayOfWeek { get; set; }
    }
}
