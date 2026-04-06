namespace AppointmentSystem.Api.Shared.Constants
{
    public static class CacheKeys
    {
        public const string CATALOG_CATEGORIES = "catalog:categories";
        public const string CATALOG_POPULAR_BUSINESSES = "catalog:popular:{0}";
        public const string CATALOG_SEARCH_BUSINESSES = "catalog:search:{0}:{1}:{2}";
        public const string AVAILABILITY_SLOTS = "availability:{0}:{1}:{2}:{3}";
        public const string WORKER_DISTRIBUTED_LOCK = "worker:appointment-state:lock";
    }
}
