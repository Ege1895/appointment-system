namespace AppointmentSystem.Api.Modules.OfferingModule.Enums
{
    public enum PriceType
    {
        Fixed = 1,          // Sabit Fiyat (Örn: Saç Kesimi - 500 TL)
        StartingAt = 2,     // -den başlayan fiyatlar (Örn: Dövme - 1000 TL'den başlayan)
        Varies = 3,         // Değişken / Görüşmede belirlenir (Örn: Dişçi Muayenesi, Tamirat)
        Free = 4,           // Ücretsiz (Örn: Ön Görüşme, Tanışma)
        NotShown = 5        // Fiyat Gösterilmez (Örn: Psikolog / Özel Klinik)
    }
}
