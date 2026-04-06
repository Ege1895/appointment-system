using System;

namespace AppointmentSystem.Api.Modules.AppointmentModule.DTOs
{
    public class MarkMessagesReadRequest
    {
        public Guid AppointmentId { get; set; }
    }
}
