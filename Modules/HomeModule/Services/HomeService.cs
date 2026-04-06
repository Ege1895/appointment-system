using AppointmentSystem.Api.Modules.AppointmentModule.DTOs;
using AppointmentSystem.Api.Modules.AppointmentModule.Services;
using AppointmentSystem.Api.Modules.CatalogModule.DTOs;
using AppointmentSystem.Api.Modules.CatalogModule.Services;
using AppointmentSystem.Api.Modules.HomeModule.DTOs;
using AppointmentSystem.Api.Shared.Authentication;
using AppointmentSystem.Api.Shared.Constants;

namespace AppointmentSystem.Api.Modules.HomeModule.Services
{
    public class HomeService : IHomeService
    {
        private readonly ICatalogService _catalogService;
        private readonly IAppointmentService _appointmentService;
        private readonly ICurrentUserService _currentUserService;

        public HomeService(
            ICatalogService catalogService,
            IAppointmentService appointmentService,
            ICurrentUserService currentUserService)
        {
            _catalogService = catalogService;
            _appointmentService = appointmentService;
            _currentUserService = currentUserService;
        }

        public async Task<HomeDashboardDto> GetDashboardAsync(int popularTake, int upcomingLimit)
        {
            IReadOnlyList<CategoryFilterItemDto> categories = await _catalogService.GetCategoryFiltersAsync();
            IReadOnlyList<PublicBusinessSummaryDto> popular =
                await _catalogService.GetPopularBusinessesAsync(popularTake);

            List<AppointmentResponseDto> upcoming = new List<AppointmentResponseDto>();

            if (_currentUserService.Role == Roles.User.ToString()
                && !string.IsNullOrEmpty(_currentUserService.UserId)
                && Guid.TryParse(_currentUserService.UserId, out _))
            {
                upcoming = await _appointmentService.GetMyUpcomingAsync(upcomingLimit);
            }

            return new HomeDashboardDto
            {
                Categories = categories,
                PopularBusinesses = popular,
                UpcomingAppointments = upcoming
            };
        }
    }
}
