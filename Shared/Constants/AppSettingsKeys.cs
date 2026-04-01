namespace AppointmentSystem.Api.Shared.Constants
{
    public static class AppSettingsKeys
    {
        public const string JWT_SECTION = "Jwt";
        public const string DEFAULT_CONNECTION = "DefaultConnection";
        public const string REDIS_CONNECTION = "Redis";
        public const string ELASTIC_URI = "Elastic:Uri";
        public const string CORS_ALLOWED_ORIGINS = "Cors:AllowedOrigins";
        public const string ELASTIC_INDEX_FORMAT = "appointmentsystem-logs-{0:yyyy.MM.dd}";
    }
}
