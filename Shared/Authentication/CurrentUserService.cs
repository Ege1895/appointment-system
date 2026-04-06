using System.Security.Claims;
using AppointmentSystem.Api.Shared.Constants;

namespace AppointmentSystem.Api.Shared.Authentication
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserId
        {
            get
            {
                return _httpContextAccessor.HttpContext?
                    .User?
                    .FindFirst(ClaimTypes.NameIdentifier)?
                    .Value;
            }
        }

        public string? Role
        {
            get
            {
                return _httpContextAccessor.HttpContext?
                    .User?
                    .FindFirst(ClaimTypes.Role)?
                    .Value;
            }
        }

        public Guid? TenantId
        {
            get
            {
                if (Role == Roles.Business.ToString() && Guid.TryParse(UserId, out Guid tenantId))
                {
                    return tenantId;
                }

                return null;
            }
        }
    }
}