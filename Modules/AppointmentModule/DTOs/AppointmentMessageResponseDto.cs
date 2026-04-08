using System;

namespace AppointmentSystem.Api.Modules.AppointmentModule.DTOs
{
    public class AppointmentMessageResponseDto
    {
        public Guid MessageId { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string SenderRole { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsReadByBusiness { get; set; }
        public bool IsReadByCustomer { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
