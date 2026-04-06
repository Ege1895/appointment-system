using System;
using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.Api.Modules.BusinessModule.DTOs
{
    public class SendCustomerEmailRequest
    {
        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Message { get; set; } = string.Empty;
    }
}
