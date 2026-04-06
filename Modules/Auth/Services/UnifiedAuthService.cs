using AppointmentSystem.Api.Modules.Auth.DTOs;
using AppointmentSystem.Api.Modules.Auth.Enums;
using AppointmentSystem.Api.Modules.BusinessModule.DTOs;
using AppointmentSystem.Api.Modules.BusinessModule.Services;
using AppointmentSystem.Api.Modules.UserModule.DTOs;
using AppointmentSystem.Api.Modules.UserModule.Services;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Exceptions;

namespace AppointmentSystem.Api.Modules.Auth.Services
{
    public class UnifiedAuthService : IUnifiedAuthService
    {
        private readonly IBusinessAuthService _businessAuthService;
        private readonly IUserAuthService _userAuthService;

        public UnifiedAuthService(
            IBusinessAuthService businessAuthService,
            IUserAuthService userAuthService)
        {
            _businessAuthService = businessAuthService;
            _userAuthService = userAuthService;
        }

        public async Task<UnifiedAuthResponseDto> AuthenticateAsync(UnifiedAuthRequest request)
        {
            if (request.AccountType == AccountType.BUSINESS)
            {
                BusinessAuthResponseDto businessResult = await _businessAuthService.LoginAsync(new LoginBusinessRequest
                {
                    Email = request.Email,
                    Password = request.Password
                });

                return new UnifiedAuthResponseDto
                {
                    Token = businessResult.Token,
                    RefreshToken = businessResult.RefreshToken,
                    Role = Roles.Business.ToString(),
                    BusinessId = businessResult.Business.BusinessId
                };
            }
            else if (request.AccountType == AccountType.CUSTOMER)
            {
                UserAuthResponseDto userResult = await _userAuthService.LoginAsync(new LoginUserRequest
                {
                    Email = request.Email,
                    Password = request.Password
                });

                return new UnifiedAuthResponseDto
                {
                    Token = userResult.Token,
                    RefreshToken = userResult.RefreshToken,
                    Role = Roles.User.ToString(),
                    BusinessId = null
                };
            }

            throw new BusinessException(ErrorMessages.INVALID_EMAIL_OR_PASSWORD);
        }
    }
}
