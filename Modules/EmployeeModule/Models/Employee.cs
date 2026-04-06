using AppointmentSystem.Api.Modules.BusinessModule.Models;
using AppointmentSystem.Api.Modules.BranchModule.Models;
using AppointmentSystem.Api.Modules.UserModule.Models;
using AppointmentSystem.Api.Shared.Interfaces;
using AppointmentSystem.Api.Shared.Models;

namespace AppointmentSystem.Api.Modules.EmployeeModule.Models
{
    public class Employee : BaseEntity, IMustHaveTenant
    {
        public Guid BusinessId { get; set; }
        public Business Business { get; set; } = null!;
        
        public Guid? UserId { get; set; }
        public User? User { get; set; }

        public Guid? BranchId { get; set; }
        public virtual Branch? Branch { get; set; }

        public bool CanManageOwnSchedule { get; set; } = false;
        
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string JobTitle { get; set; }
        public required string JobDescription { get; set; }
    }
}
