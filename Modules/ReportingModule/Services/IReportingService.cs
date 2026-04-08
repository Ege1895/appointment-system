using AppointmentSystem.Api.Modules.ReportingModule.DTOs;
using System.Threading.Tasks;

namespace AppointmentSystem.Api.Modules.ReportingModule.Services
{
    public interface IReportingService
    {
        Task<AppointmentCountReportResponseDto> GetAppointmentCountReportAsync(AppointmentCountReportRequest request);
        Task<RevenueReportResponseDto> GetRevenueReportAsync(RevenueReportRequest request);
    }
}
