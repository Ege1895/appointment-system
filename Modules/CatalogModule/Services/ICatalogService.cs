using AppointmentSystem.Api.Modules.CatalogModule.DTOs;
using Shared.Responses;

namespace AppointmentSystem.Api.Modules.CatalogModule.Services
{
    public interface ICatalogService
    {
        Task<IReadOnlyList<CategoryFilterItemDto>> GetCategoryFiltersAsync();

        Task<(IReadOnlyList<PublicBusinessSummaryDto> Items, PaginationMeta Meta)> SearchBusinessesAsync(
            int? categoryFlag,
            int page,
            int pageSize);

        Task<IReadOnlyList<PublicBusinessSummaryDto>> GetPopularBusinessesAsync(int take);
    }
}
