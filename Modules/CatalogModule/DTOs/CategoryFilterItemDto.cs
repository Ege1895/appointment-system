namespace AppointmentSystem.Api.Modules.CatalogModule.DTOs
{
    /// <summary>
    /// Ana ekran kategori chip'i; <see cref="Value"/> <c>Business.Category</c> bayraklarıyla eşleşir (AND ile filtre).
    /// </summary>
    public class CategoryFilterItemDto
    {
        public int Value { get; set; }

        public required string Name { get; set; }
    }
}
