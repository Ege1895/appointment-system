using System;

namespace AppointmentSystem.Api.Modules.ReportingModule.DTOs
{
    public class RevenueBucketDto
    {
        public string PeriodLabel { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public decimal TotalRevenue { get; set; }
        public int AppointmentCount { get; set; }
        public decimal AverageRevenue => AppointmentCount == 0 ? 0 : TotalRevenue / AppointmentCount;
    }
}
