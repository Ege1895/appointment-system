using AppointmentSystem.Api.Modules.BusinessModule.DTOs;

namespace AppointmentSystem.Api.Modules.BusinessModule.Services
{
    public interface IBusinessService
    {
        Task<BusinessResponseDto> GetProfileAsync();
        
        Task UpdateProfileAsync(UpdateBusinessProfileRequest request);
    }
}
