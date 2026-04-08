using System;

namespace AppointmentSystem.Api.Modules.EmployeeModule.DTOs
{
    public class EmployeeOfferingResponseDto
    {
        public Guid EmployeeId { get; set; }
        public Guid OfferingId { get; set; }
        public string OfferingName { get; set; } = string.Empty;
        public int DurationInMinutes { get; set; }
    }
}
