using System;
using System.Collections.Generic;

namespace AppointmentSystem.Api.Modules.ReportingModule.DTOs
{
    public class PopularServicesResponseDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalBookingsInPeriod { get; set; }
        public List<ServicePopularityDto> Services { get; set; } = new();
        public ServicePopularityDto? MostPopular { get; set; }
        public ServicePopularityDto? MostRevenue { get; set; }
    }
}
