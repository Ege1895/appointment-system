using AppointmentSystem.Api.Modules.PromotionModule.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppointmentSystem.Api.Modules.PromotionModule.Services
{
    public interface IPromotionService
    {
        Task<List<BoostPackageDto>> GetBoostPackagesAsync();
        Task<BusinessBoostResponseDto> PurchaseBoostAsync(Guid businessId, PurchaseBoostRequest request);
        Task<BusinessBoostResponseDto?> GetActiveBoostAsync(Guid businessId);
        Task<bool> IsBusinessBoostedAsync(Guid businessId);
    }
}
