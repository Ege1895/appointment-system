using System;

namespace AppointmentSystem.Api.Modules.EmployeeModule.DTOs
{
    public class EmployeeResponseDto
    {
        public Guid EmployeeId { get; set; }
        public Guid BusinessId { get; set; }
        
        public Guid? UserId { get; set; }

        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string JobTitle { get; set; }
        public required string JobDescription { get; set; }

        public string? Email { get; set; }

        public bool CanManageOwnSchedule { get; set; }
        
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
