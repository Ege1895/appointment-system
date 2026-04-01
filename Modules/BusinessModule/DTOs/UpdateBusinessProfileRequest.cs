using System;
using System.Collections.Generic;
using AppointmentSystem.Api.Modules.BusinessModule.Enums;

namespace AppointmentSystem.Api.Modules.BusinessModule.DTOs
{
    public class UpdateBusinessProfileRequest
    {
        public TimeSpan? WorkingHourStart { get; set; }
        public TimeSpan? WorkingHourEnd { get; set; }
        public List<DayOfWeek>? WorkingDays { get; set; }
        
        public List<string>? GalleryUrls { get; set; } = new List<string>();

        public BusinessCategory Category { get; set; }
        
        public string? City { get; set; }
        public string? District { get; set; }
        public string? Neighborhood { get; set; }
        public string? Description { get; set; }

        public bool IsOpen { get; set; }
    }
}
