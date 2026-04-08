namespace AppointmentSystem.Api.Modules.BusinessModule.DTOs
{
    public class ExportCustomersRequest
    {
        public ExportFormat Format { get; set; } = ExportFormat.Csv;
    }
}
