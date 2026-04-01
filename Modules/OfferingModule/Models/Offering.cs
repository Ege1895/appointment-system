using AppointmentSystem.Api.Modules.BusinessModule.Models;
using AppointmentSystem.Api.Modules.OfferingModule.Enums;
using AppointmentSystem.Api.Shared.Interfaces;
using AppointmentSystem.Api.Shared.Models;

namespace AppointmentSystem.Api.Modules.OfferingModule.Models
{
    public class Offering : BaseEntity, IMustHaveTenant
    {
        public Guid BusinessId { get; set; }
        public Business Business { get; set; } = null!;

        public required string Name { get; set; }
        public string? Description { get; set; }
        public int DurationInMinutes { get; set; }
        public PriceType PriceType { get; set; }
        public decimal? Price { get; set; }
        public bool IsOnline { get; set; }
    }
}
