using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppointmentSystem.Api.Modules.BusinessModule.DTOs;
using Shared.Responses;

namespace AppointmentSystem.Api.Modules.BusinessModule.Services
{
    public interface IBusinessCustomerService
    {
        Task<(IReadOnlyList<BusinessCustomerSummaryDto> Items, PaginationMeta Meta)> GetCustomersAsync(GetCustomersQuery query);
        Task<BusinessCustomerDetailDto> GetCustomerDetailAsync(Guid customerId);

        Task BlockCustomerAsync(BlockCustomerRequest request);
        Task UnblockCustomerAsync(Guid userId);
        Task<List<BlockedCustomerResponseDto>> GetBlockedCustomersAsync();
        Task<bool> IsBlockedAsync(Guid businessId, Guid userId);

        Task SendEmailToCustomerAsync(SendCustomerEmailRequest request);
    }
}
