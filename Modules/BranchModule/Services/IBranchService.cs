using AppointmentSystem.Api.Modules.BranchModule.DTOs;

namespace AppointmentSystem.Api.Modules.BranchModule.Services
{
    public interface IBranchService
    {
        /// <summary>
        /// Yeni bir şube oluşturur.
        /// </summary>
        Task<BranchResponseDto> CreateAsync(CreateBranchRequest request);

        /// <summary>
        /// İşletmeye ait tüm aktif şubeleri listeler.
        /// </summary>
        Task<List<BranchResponseDto>> GetAllAsync();

        /// <summary>
        /// ID bazlı şube detayı getirir.
        /// </summary>
        Task<BranchResponseDto> GetByIdAsync(Guid branchId);

        /// <summary>
        /// Şube bilgilerini günceller.
        /// </summary>
        Task UpdateAsync(Guid branchId, UpdateBranchRequest request);

        /// <summary>
        /// Şubeyi siler (Soft-delete).
        /// </summary>
        Task DeleteAsync(Guid branchId);

        /// <summary>
        /// Belirtilen şubeyi işletmenin ana şubesi olarak ayarlar.
        /// </summary>
        Task SetMainBranchAsync(Guid branchId);
    }
}
