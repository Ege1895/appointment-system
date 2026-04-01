using AppointmentSystem.Api.Modules.BusinessModule.Enums;

namespace AppointmentSystem.Api.Modules.CatalogModule.DTOs
{
    public class PublicBusinessSummaryDto
    {
        public Guid BusinessId { get; set; }

        public required string CompanyName { get; set; }

        public required string Subdomain { get; set; }

        public BusinessCategory Category { get; set; }

        public string? City { get; set; }

        public string? District { get; set; }

        public decimal AverageRating { get; set; }

        public int ReviewCount { get; set; }

        public bool IsOpen { get; set; }

        public string? ThumbnailUrl { get; set; }
    }
}
