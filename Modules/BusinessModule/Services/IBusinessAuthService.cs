using AppointmentSystem.Api.Modules.BusinessModule.DTOs;
using AppointmentSystem.Api.Shared.Requests;

namespace AppointmentSystem.Api.Modules.BusinessModule.Services
{
    public interface IBusinessAuthService
    {
        Task<BusinessAuthResponseDto> RegisterAsync(RegisterBusinessRequest request);
        Task<BusinessAuthResponseDto> LoginAsync(LoginBusinessRequest request);
        Task<BusinessAuthResponseDto> RefreshTokenAsync(RefreshTokenRequest request);
        Task LogoutAsync(Guid businessId);
    }
}
