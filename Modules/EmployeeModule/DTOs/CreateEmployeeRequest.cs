using System;

namespace AppointmentSystem.Api.Modules.EmployeeModule.DTOs
{
    public class CreateEmployeeRequest
    {
        public required string Email { get; set; }
        public required string JobTitle { get; set; }
        public required string JobDescription { get; set; }
        
        public bool CanManageOwnSchedule { get; set; } = false;
    }
}
