using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using AppointmentSystem.Api.Modules.BusinessModule.DTOs;

namespace AppointmentSystem.Api.Modules.BusinessModule.Services
{
    public interface IImportService
    {
        Task<ImportResultDto> ImportEmployeesAsync(IFormFile file);
        Task<ImportResultDto> ImportCustomersAsync(IFormFile file);
    }
}
