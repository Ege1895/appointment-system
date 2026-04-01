using AppointmentSystem.Api.Modules.UserModule.DTOs;
using AppointmentSystem.Api.Shared.Requests;

namespace AppointmentSystem.Api.Modules.UserModule.Services
{
    public interface IUserAuthService
    {
        Task<UserAuthResponseDto> RegisterAsync(RegisterUserRequest request);
        Task<UserAuthResponseDto> LoginAsync(LoginUserRequest request);
        Task<UserAuthResponseDto> RefreshTokenAsync(RefreshTokenRequest request);
        Task LogoutAsync(Guid userId);
    }
}
