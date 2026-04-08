using AppointmentSystem.Api.Modules.OfferingModule.Models;
using AppointmentSystem.Api.Shared.Interfaces;
using AppointmentSystem.Api.Shared.Models;

namespace AppointmentSystem.Api.Modules.EmployeeModule.Models
{
    public class EmployeeOffering : BaseEntity, IMustHaveTenant
    {
        public Guid BusinessId { get; set; }
        
        public Guid EmployeeId { get; set; }
        public virtual Employee Employee { get; set; } = null!;
        
        public Guid OfferingId { get; set; }
        public virtual Offering Offering { get; set; } = null!;
    }
}
