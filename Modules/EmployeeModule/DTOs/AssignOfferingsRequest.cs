using System;
using System.Collections.Generic;

namespace AppointmentSystem.Api.Modules.EmployeeModule.DTOs
{
    public class AssignOfferingsRequest
    {
        public List<Guid> OfferingIds { get; set; } = new();
    }
}
