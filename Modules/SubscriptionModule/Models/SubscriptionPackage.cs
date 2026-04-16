namespace AppointmentSystem.Api.Modules.SubscriptionModule.Models
{
    public class SubscriptionPackage
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        
        public decimal PriceMonthly { get; set; }
        public decimal? PriceYearly { get; set; }
        
        // Limitler (0 = sınırsız)
        public int MaxEmployees { get; set; }
        public int MaxBranches { get; set; }
        public int MaxOfferings { get; set; }
        public int MaxMonthlyAppointments { get; set; }
        
        // Özellik Erişimleri
        public bool CampaignsEnabled { get; set; }
        public bool LoyaltyEnabled { get; set; }
        public bool ExportEnabled { get; set; }
        public bool AnalyticsEnabled { get; set; }
        public bool PromotionEnabled { get; set; }
        
        public bool IsPublic { get; set; } = true;
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
