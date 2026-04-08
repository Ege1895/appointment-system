using System;

namespace AppointmentSystem.Api.Modules.ReportingModule.DTOs
{
    public class BranchStatsRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
