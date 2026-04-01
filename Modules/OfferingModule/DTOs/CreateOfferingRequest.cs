using AppointmentSystem.Api.Modules.OfferingModule.Enums;

namespace AppointmentSystem.Api.Modules.OfferingModule.DTOs
{
    public class CreateOfferingRequest
    {

        public PriceType PriceType { get; set; }

        public required string Name { get; set; }

        public string? Description { get; set; }

        public int DurationInMinutes { get; set; }

        public decimal? Price { get; set; }

        public bool IsOnline { get; set; }
    }
}
