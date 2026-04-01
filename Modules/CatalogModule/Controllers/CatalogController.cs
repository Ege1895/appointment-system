using AppointmentSystem.Api.Modules.CatalogModule.DTOs;
using AppointmentSystem.Api.Modules.CatalogModule.Services;
using AppointmentSystem.Api.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Responses;

namespace AppointmentSystem.Api.Modules.CatalogModule.Controllers
{
    [ApiController]
    [Route("api/v1/catalog")]
    [AllowAnonymous]
    public class CatalogController : ControllerBase
    {
        private readonly ICatalogService _catalogService;

        public CatalogController(ICatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        /// <summary>Ana ekran kategori filtreleri (BusinessCategory bayrak değerleri).</summary>
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            IReadOnlyList<CategoryFilterItemDto> items = await _catalogService.GetCategoryFiltersAsync();
            return Ok(ApiResponse<IReadOnlyList<CategoryFilterItemDto>>.SuccessResponse(items));
        }

        /// <summary>
        /// İşletme listesi. <paramref name="category"/> verilirse ilgili ana kategori bayrağına göre filtrelenir (ör. 128 = Eğitim).
        /// </summary>
        [HttpGet("businesses")]
        public async Task<IActionResult> SearchBusinesses(
            [FromQuery] int? category,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            (IReadOnlyList<PublicBusinessSummaryDto> items, PaginationMeta meta) =
                await _catalogService.SearchBusinessesAsync(category, page, pageSize);

            return Ok(ApiResponse<IReadOnlyList<PublicBusinessSummaryDto>>.SuccessResponse(items, meta));
        }

        /// <summary>Yüksek puanlı / çok değerlendirilmiş işletmeler (önce AverageRating, sonra ReviewCount).</summary>
        [HttpGet("popular-businesses")]
        public async Task<IActionResult> GetPopularBusinesses([FromQuery] int take = 10)
        {
            IReadOnlyList<PublicBusinessSummaryDto> items = await _catalogService.GetPopularBusinessesAsync(take);
            return Ok(ApiResponse<IReadOnlyList<PublicBusinessSummaryDto>>.SuccessResponse(items));
        }
    }
}
