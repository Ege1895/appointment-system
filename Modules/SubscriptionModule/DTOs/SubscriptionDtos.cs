using AppointmentSystem.Api.Modules.SubscriptionModule.Enums;

namespace AppointmentSystem.Api.Modules.SubscriptionModule.DTOs
{
    public class SubscriptionPackageDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal PriceMonthly { get; set; }
        public decimal? PriceYearly { get; set; }
        public int MaxEmployees { get; set; }
        public int MaxBranches { get; set; }
        public int MaxOfferings { get; set; }
        public int MaxMonthlyAppointments { get; set; }
        
        public bool CampaignsEnabled { get; set; }
        public bool LoyaltyEnabled { get; set; }
        public bool ExportEnabled { get; set; }
        public bool AnalyticsEnabled { get; set; }
        public bool PromotionEnabled { get; set; }
        public int SortOrder { get; set; }
    }

    public class BusinessSubscriptionDto
    {
        public Guid BusinessId { get; set; }
        public Guid PackageId { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; }
        public BillingCycle BillingCycle { get; set; }
        
        // Flattened Limits for easy access in UI
        public int MaxEmployees { get; set; }
        public int MaxBranches { get; set; }
        public bool CampaignsEnabled { get; set; }
        public bool LoyaltyEnabled { get; set; }
        public bool AnalyticsEnabled { get; set; }
    }

    public class UpgradePackageRequest
    {
        public Guid ToPackageId { get; set; }
        public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;
    }

    public class UpgradePackageResponseDto
    {
        public Guid ChangeRequestId { get; set; }
        public string FromPackageName { get; set; } = string.Empty;
        public string ToPackageName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool ActivatedImmediately { get; set; }
        public DateTime? NewExpiresAt { get; set; }
    }

    public class SubscriptionInfo
    {
        public string PackageName { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
        public int MaxEmployees { get; set; }
        public bool CampaignsEnabled { get; set; }
        public bool LoyaltyEnabled { get; set; }
        public bool AnalyticsEnabled { get; set; }
    }
}
