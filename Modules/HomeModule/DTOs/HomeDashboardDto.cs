using AppointmentSystem.Api.Modules.AppointmentModule.DTOs;
using AppointmentSystem.Api.Modules.CatalogModule.DTOs;

namespace AppointmentSystem.Api.Modules.HomeModule.DTOs
{
    public class HomeDashboardDto
    {
        public IReadOnlyList<CategoryFilterItemDto> Categories { get; set; } = Array.Empty<CategoryFilterItemDto>();

        /// <summary>Öne çıkan işletmeler (puan / yorum sayısı).</summary>
        public IReadOnlyList<PublicBusinessSummaryDto> PopularBusinesses { get; set; } = Array.Empty<PublicBusinessSummaryDto>();

        /// <summary>
        /// Giriş yapmış müşteri (<c>User</c> rolü) için yaklaşan randevular; aksi halde boş liste.
        /// </summary>
        public IReadOnlyList<AppointmentResponseDto> UpcomingAppointments { get; set; } = Array.Empty<AppointmentResponseDto>();
    }
}
