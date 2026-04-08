using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Interfaces;
using AppointmentSystem.Api.Shared.Models;

namespace AppointmentSystem.Api.Modules.AppointmentModule.Models
{
    public class AppointmentNote : BaseEntity, IMustHaveTenant
    {
        public Guid BusinessId { get; set; }
        
        public Guid AppointmentId { get; set; }
        public virtual Appointment Appointment { get; set; } = null!;
        
        public string AuthorId { get; set; } = string.Empty;
        public string AuthorRole { get; set; } = string.Empty;
        
        public string Content { get; set; } = string.Empty;
        
        public DateTime? UpdatedAt { get; set; }
    }
}
