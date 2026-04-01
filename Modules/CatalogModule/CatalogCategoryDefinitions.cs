using AppointmentSystem.Api.Modules.BusinessModule.Enums;
using AppointmentSystem.Api.Modules.CatalogModule.DTOs;

namespace AppointmentSystem.Api.Modules.CatalogModule
{
    /// <summary>
    /// <see cref="BusinessCategory"/> bayrakları için sabit ana kategori listesi (UI filtreleri).
    /// </summary>
    public static class CatalogCategoryDefinitions
    {
        public static IReadOnlyList<CategoryFilterItemDto> FilterItems { get; } =
            new List<CategoryFilterItemDto>
            {
                new() { Value = (int)BusinessCategory.Healthcare, Name = "Sağlık" },
                new() { Value = (int)BusinessCategory.Law, Name = "Hukuk" },
                new() { Value = (int)BusinessCategory.BeautyAndCare, Name = "Güzellik ve Bakım" },
                new() { Value = (int)BusinessCategory.Sports, Name = "Spor" },
                new() { Value = (int)BusinessCategory.Consulting, Name = "Danışmanlık" },
                new() { Value = (int)BusinessCategory.Art, Name = "Sanat" },
                new() { Value = (int)BusinessCategory.FoodAndBeverage, Name = "Yeme İçme" },
                new() { Value = (int)BusinessCategory.Education, Name = "Eğitim" },
                new() { Value = (int)BusinessCategory.Engineering, Name = "Mühendislik" }
            };
    }
}
