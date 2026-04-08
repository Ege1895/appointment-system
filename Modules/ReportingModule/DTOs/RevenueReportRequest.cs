using System;

namespace AppointmentSystem.Api.Modules.ReportingModule.DTOs
{
    public class RevenueReportRequest
    {
        public ReportPeriod Period { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid? BranchId { get; set; }
    }
}
