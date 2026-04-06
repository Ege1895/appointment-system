using System.Collections.Generic;
using AppointmentSystem.Api.Modules.AppointmentModule.DTOs;

namespace AppointmentSystem.Api.Modules.BusinessModule.DTOs
{
    public class BusinessCustomerDetailDto : BusinessCustomerSummaryDto
    {
        public List<AppointmentResponseDto> RecentAppointments { get; set; } = new List<AppointmentResponseDto>();
    }
}
