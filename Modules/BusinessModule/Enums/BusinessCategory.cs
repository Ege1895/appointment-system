namespace AppointmentSystem.Api.Modules.BusinessModule.Enums
{
    [Flags]
    public enum BusinessCategory
    {
        None = 0,
        Healthcare = 1 << 0,          // 1
        Law = 1 << 1,                 // 2
        BeautyAndCare = 1 << 2,       // 4
        Sports = 1 << 3,              // 8
        Consulting = 1 << 4,          // 16
        Art = 1 << 5,                 // 32
        FoodAndBeverage = 1 << 6,     // 64
        Education = 1 << 7,           // 128
        Engineering = 1 << 8          // 256
    }
}
