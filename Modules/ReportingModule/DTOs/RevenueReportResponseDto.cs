using System;
using System.Collections.Generic;
using System.Linq;

namespace AppointmentSystem.Api.Modules.ReportingModule.DTOs
{
    public class RevenueReportResponseDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalCompletedAppointments { get; set; }
        public decimal AverageRevenuePerAppointment => TotalCompletedAppointments == 0 ? 0 : TotalRevenue / TotalCompletedAppointments;
        public string CurrencyCode { get; set; } = "TRY";
        public List<RevenueBucketDto> Buckets { get; set; } = new();
        public List<RevenueByServiceDto> TopServices { get; set; } = new();
        public bool HasPricedOfferings { get; set; } = true;
    }
}
