using AppointmentSystem.Api.Modules.AppointmentModule.Enums;

namespace AppointmentSystem.Api.Modules.BusinessModule.DTOs
{
    public class ExportAppointmentsRequest
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public AppointmentStatus? Status { get; set; }
        public Guid? BranchId { get; set; }
        public ExportFormat Format { get; set; } = ExportFormat.Csv;
    }
}
