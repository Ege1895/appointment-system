using AppointmentSystem.Api.Modules.BusinessModule.DTOs;

namespace AppointmentSystem.Api.Modules.BusinessModule.Services
{
    public interface IExportService
    {
        Task<(byte[] Content, string FileName, string ContentType)> ExportAppointmentsAsync(ExportAppointmentsRequest request);
        Task<(byte[] Content, string FileName, string ContentType)> ExportCustomersAsync(ExportCustomersRequest request);
    }
}
