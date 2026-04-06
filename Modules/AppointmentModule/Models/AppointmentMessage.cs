using AppointmentSystem.Api.Shared.Interfaces;
using AppointmentSystem.Api.Shared.Models;

namespace AppointmentSystem.Api.Modules.AppointmentModule.Models
{
    public class AppointmentMessage : BaseEntity, IMustHaveTenant
    {
        public Guid BusinessId { get; set; }
        
        public Guid AppointmentId { get; set; }
        public virtual Appointment Appointment { get; set; } = null!;
        
        public string SenderId { get; set; } = string.Empty;
        public string SenderRole { get; set; } = string.Empty;
        
        public string Content { get; set; } = string.Empty;
        
        public bool IsReadByBusiness { get; set; }
        public bool IsReadByCustomer { get; set; }
    }
}
