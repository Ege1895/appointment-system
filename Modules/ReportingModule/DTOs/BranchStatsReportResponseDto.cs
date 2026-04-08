using System;
using System.Collections.Generic;

namespace AppointmentSystem.Api.Modules.ReportingModule.DTOs
{
    public class BranchStatsReportResponseDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<BranchStatDto> Branches { get; set; } = new();
        public Guid? BusiestBranchId { get; set; }
        public int TotalAcrossAllBranches { get; set; }
    }
}
