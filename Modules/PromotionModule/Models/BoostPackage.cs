using System;
using System.ComponentModel.DataAnnotations;
using AppointmentSystem.Api.Shared.Models;

namespace AppointmentSystem.Api.Modules.PromotionModule.Models
{
    public class BoostPackage : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public int DurationDays { get; set; }

        public decimal Price { get; set; }

        public int BoostScore { get; set; }

        public int SortOrder { get; set; }
    }
}
