using AppointmentSystem.Api.Modules.OfferingModule.Enums;
using System;

namespace AppointmentSystem.Api.Modules.ReportingModule.DTOs
{
    public class ServicePopularityDto
    {
        public Guid OfferingId { get; set; }
        public string OfferingName { get; set; } = string.Empty;
        public PriceType PriceType { get; set; }
        public decimal? Price { get; set; }
        public int TotalBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public double CompletionRate => TotalBookings == 0 ? 0 : (double)CompletedBookings / TotalBookings * 100;
        public decimal TotalRevenue { get; set; }
        public double BookingSharePercent { get; set; }
        public double AverageBookingsPerDay { get; set; }
    }
}
