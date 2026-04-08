using System;

namespace AppointmentSystem.Api.Modules.ReportingModule.DTOs
{
    public class PopularServicesRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Top { get; set; } = 10;
        public Guid? BranchId { get; set; }
    }
}
