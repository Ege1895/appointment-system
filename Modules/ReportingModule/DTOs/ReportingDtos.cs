using System;
using System.Collections.Generic;

namespace AppointmentSystem.Api.Modules.ReportingModule.DTOs
{
    public enum ReportPeriod
    {
        Daily,
        Weekly,
        Monthly,
        Yearly
    }

    public class AppointmentCountReportRequest
    {
        public ReportPeriod Period { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid? BranchId { get; set; }
    }

    public class AppointmentCountBucketDto
    {
        public string PeriodLabel { get; set; } = string.Empty; // "2026-04-06", "2026-W14" vb.
        public DateTime PeriodStart { get; set; }
        public int Total { get; set; }
        public int Confirmed { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
        public int Declined { get; set; }
        public int PendingApproval { get; set; }
    }

    public class AppointmentCountReportResponseDto
    {
        public Guid BusinessId { get; set; }
        public string Period { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalInRange { get; set; }
        public List<AppointmentCountBucketDto> Buckets { get; set; } = new();
        public string Timezone { get; set; } = "UTC";
    }
}
