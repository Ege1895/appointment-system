using System;
using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.Api.Modules.BusinessModule.DTOs
{
    public class BlockCustomerRequest
    {
        [Required]
        public Guid UserId { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }
    }
}
