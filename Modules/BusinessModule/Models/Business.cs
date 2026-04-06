using AppointmentSystem.Api.Shared.Models;
using AppointmentSystem.Api.Modules.BusinessModule.Enums;
namespace AppointmentSystem.Api.Modules.BusinessModule.Models
{
    public class Business : BaseEntity
    {
        public BusinessCategory Category { get; set; } = BusinessCategory.None;

        public TimeSpan? WorkingHourStart { get; set; }
        public TimeSpan? WorkingHourEnd { get; set; }
        public List<DayOfWeek>? WorkingDays { get; set; }
        
        public List<string>? GalleryUrls { get; set; } = new List<string>();
        
        public DateTime? RefreshTokenExpiry { get; set; }

        public required string CompanyName { get; set; }
        public required string Subdomain { get; set; }
        public required string OwnerFirstName { get; set; }
        public required string OwnerLastName { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public required string ContactPhone { get; set; }
        
        
        public string? City { get; set; }
        public string? District { get; set; }
        public string? Neighborhood { get; set; }
        public string? Description { get; set; }
        public string? RefreshTokenHash { get; set; }

        public int EmployeeCapacity { get; set; }

        /// <summary>0–5 arası; değerlendirme modülü gelene kadar 0 olabilir.</summary>
        public decimal AverageRating { get; set; }

        public int ReviewCount { get; set; }
        
        public bool IsOpen { get; set; } = true;
        public bool RefreshTokenUsed { get; set; } = false;

        public bool KvkkAccepted { get; set; }
        public bool PrivacyPolicyAccepted { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public string? KvkkVersion { get; set; }
        public string? PrivacyVersion { get; set; }
    }
}
