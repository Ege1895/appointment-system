using System;
using System.Collections.Generic;

namespace AppointmentSystem.Api.Modules.BusinessModule.DTOs
{
    public class BusinessCustomerSummaryDto
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public DateTime? LastAppointmentDate { get; set; }
        public DateTime? FirstAppointmentDate { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }
}
