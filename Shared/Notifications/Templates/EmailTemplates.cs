namespace AppointmentSystem.Api.Shared.Notifications.Templates
{
    public static class EmailTemplates
    {
        public const string APPOINTMENT_CONFIRMED_SUBJECT = "Randevunuz Onaylandı - {0}";
        public const string APPOINTMENT_CONFIRMED_BODY = @"
            <h1>Merhaba {0},</h1>
            <p><strong>{1}</strong> işletmesindeki randevunuz onaylanmıştır.</p>
            <p><strong>Hizmet:</strong> {2}</p>
            <p><strong>Tarih:</strong> {3}</p>
            <p>Sizinle görüşmek için sabırsızlanıyoruz!</p>";

        public const string APPOINTMENT_DECLINED_SUBJECT = "Randevu Talebiniz Hakkında - {0}";
        public const string APPOINTMENT_DECLINED_BODY = @"
            <h1>Merhaba {0},</h1>
            <p>Maalesef <strong>{1}</strong> işletmesi randevu talebinizi şu an için onaylayamadı.</p>
            <p><strong>Hizmet:</strong> {2}</p>
            <p><strong>Tarih:</strong> {3}</p>
            <p>Lütfen farklı bir saat dilimi için tekrar deneyiniz.</p>";

        public const string APPOINTMENT_CANCELLED_SUBJECT = "Randevunuz İptal Edildi - {0}";
        public const string APPOINTMENT_CANCELLED_BODY = @"
            <h1>Merhaba {0},</h1>
            <p><strong>{1}</strong> işletmesindeki randevunuz iptal edilmiştir.</p>
            <p><strong>Hizmet:</strong> {2}</p>
            <p><strong>Tarih:</strong> {3}</p>
            <p>Anlayışınız için teşekkür ederiz.</p>";

        public const string APPOINTMENT_REMINDER_SUBJECT = "Randevu Hatırlatması - {0}";
        public const string APPOINTMENT_REMINDER_BODY = @"
            <h1>Merhaba {0},</h1>
            <p><strong>{1}</strong> işletmesindeki randevunuza 24 saat kalmıştır.</p>
            <p><strong>Hizmet:</strong> {2}</p>
            <p><strong>Tarih:</strong> {3}</p>
            <p>Sizi bekliyoruz!</p>";

        public const string MANUAL_MESSAGE_SUBJECT = "{0} İşletmesinden Mesajınız Var";
        public const string MANUAL_MESSAGE_BODY = @"
            <h1>Merhaba {0},</h1>
            <p><strong>{1}</strong> işletmesi size bir mesaj gönderdi:</p>
            <div style='border-left: 4px solid #eee; padding-left: 15px; font-style: italic;'>
                {2}
            </div>
            <p>Bu e-posta platform üzerinden otomatik olarak gönderilmiştir.</p>";

        public static string Format(string template, params object[] args)
        {
            return string.Format(template, args);
        }
    }
}
