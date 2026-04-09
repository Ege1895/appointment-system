using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppointmentSystem.Api.Shared.Models;

namespace AppointmentSystem.Api.Modules.PromotionModule.Models
{
    public class BusinessBoost : BaseEntity
    {
        public Guid BusinessId { get; set; }

        public Guid BoostPackageId { get; set; }

        [ForeignKey("BoostPackageId")]
        public virtual BoostPackage Package { get; set; } = null!;

        public DateTime StartsAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        [MaxLength(100)]
        public string? PaymentReference { get; set; }
    }
}
