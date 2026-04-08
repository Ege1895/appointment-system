using System;

namespace AppointmentSystem.Api.Modules.ReportingModule.DTOs
{
    public class RevenueByServiceDto
    {
        public Guid OfferingId { get; set; }
        public string OfferingName { get; set; } = string.Empty;
        public int CompletedCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AveragePrice => CompletedCount == 0 ? 0 : TotalRevenue / CompletedCount;
    }
}
