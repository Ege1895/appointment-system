using System;

namespace AppointmentSystem.Api.Modules.PromotionModule.DTOs
{
    public class BoostPackageDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public decimal Price { get; set; }
        public int BoostScore { get; set; }
        public bool IsActive { get; set; }
    }

    public class PurchaseBoostRequest
    {
        public Guid BoostPackageId { get; set; }
    }

    public class BusinessBoostResponseDto
    {
        public Guid BoostId { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public DateTime StartsAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsCurrentlyActive { get; set; }
        public int BoostScore { get; set; }
    }
}
