using AppointmentSystem.Api.Modules.Auth.DTOs;

namespace AppointmentSystem.Api.Modules.Auth.Services
{
    public interface IUnifiedAuthService
    {
        Task<UnifiedAuthResponseDto> AuthenticateAsync(UnifiedAuthRequest request);
    }
}
