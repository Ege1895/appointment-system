using System;

namespace AppointmentSystem.Api.Modules.AppointmentModule.DTOs
{
    public class CreateAppointmentNoteRequest
    {
        public Guid AppointmentId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
