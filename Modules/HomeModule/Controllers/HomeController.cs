using AppointmentSystem.Api.Modules.HomeModule.DTOs;
using AppointmentSystem.Api.Modules.HomeModule.Services;
using AppointmentSystem.Api.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentSystem.Api.Modules.HomeModule.Controllers
{
    [ApiController]
    [Route("api/v1/home")]
    [AllowAnonymous]
    public class HomeController : ControllerBase
    {
        private readonly IHomeService _homeService;

        public HomeController(IHomeService homeService)
        {
            _homeService = homeService;
        }

        /// <summary>
        /// Mobil ana ekran: kategoriler, popüler işletmeler, (opsiyonel) yaklaşan randevular.
        /// Müşteri randevuları için istemci Bearer token gönderebilir; gönderilmezse randevu listesi boş döner.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDashboard(
            [FromQuery] int popularTake = 10,
            [FromQuery] int upcomingLimit = 5)
        {
            HomeDashboardDto dto = await _homeService.GetDashboardAsync(popularTake, upcomingLimit);
            return Ok(ApiResponse<HomeDashboardDto>.SuccessResponse(dto));
        }
    }
}
