using AppointmentSystem.Api.Modules.OfferingModule.DTOs;
using Shared.Responses;

namespace AppointmentSystem.Api.Modules.OfferingModule.Services
{
    public interface IOfferingService
    {
        Task<OfferingResponseDto> CreateAsync(CreateOfferingRequest request);
        Task<(List<OfferingResponseDto> Items, PaginationMeta Meta)> GetAllForBusinessAsync(int page, int pageSize);
    }
}
