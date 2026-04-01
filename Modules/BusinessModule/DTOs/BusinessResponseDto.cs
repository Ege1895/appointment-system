using System;
using AppointmentSystem.Api.Modules.BusinessModule.Enums;

namespace AppointmentSystem.Api.Modules.BusinessModule.DTOs
{
    public class BusinessResponseDto
    {
        public Guid BusinessId { get; set; }

        public TimeSpan? WorkingHourStart { get; set; }
        public TimeSpan? WorkingHourEnd { get; set; }
        public List<DayOfWeek>? WorkingDays { get; set; }
        
        public List<string>? GalleryUrls { get; set; } = new List<string>();
        
        public DateTime CreatedAt { get; set; }

        public required string CompanyName { get; set; }
        public required string Subdomain { get; set; }
        public required string OwnerFirstName { get; set; }
        public required string OwnerLastName { get; set; }
        public required string Email { get; set; }
        public required string ContactPhone { get; set; }

        public BusinessCategory Category { get; set; }

        public string? City { get; set; }
        public string? District { get; set; }
        public string? Neighborhood { get; set; }
        public string? Description { get; set; }

        public int EmployeeCapacity { get; set; }

        public bool IsOpen { get; set; }
    }
}
