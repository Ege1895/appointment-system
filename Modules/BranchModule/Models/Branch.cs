using AppointmentSystem.Api.Shared.Models;
using AppointmentSystem.Api.Modules.BusinessModule.Models;

namespace AppointmentSystem.Api.Modules.BranchModule.Models
{
    /// <summary>
    /// İşletmeye ait fiziksel veya mantıksal şubeyi temsil eder.
    /// her şube bir işletmeye (Business) bağlıdır.
    /// </summary>
    public class Branch : BaseEntity
    {
        /// <summary>
        /// Şubenin bağlı olduğu işletme ID'si (Multi-tenancy anahtarı)
        /// </summary>
        public Guid BusinessId { get; set; }
        
        /// <summary>
        /// Şube adı (Örn: "Kadıköy Şubesi")
        /// </summary>
        public required string Name { get; set; }
        
        /// <summary>
        /// Şube adresi
        /// </summary>
        public string? Address { get; set; }
        
        /// <summary>
        /// Şehrin adı
        /// </summary>
        public string? City { get; set; }
        
        /// <summary>
        /// İlçenin adı
        /// </summary>
        public string? District { get; set; }
        
        /// <summary>
        /// Şube iletişim numarası
        /// </summary>
        public string? PhoneNumber { get; set; }
        
        /// <summary>
        /// Bu şube işletmenin ana/varsayılan şubesi mi?
        /// </summary>
        public bool IsMain { get; set; }

        // Navigation Properties
        public virtual Business? Business { get; set; }
    }
}
