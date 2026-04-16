using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace AppointmentSystem.Api.Shared.Caching
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;

        public CacheService(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            string? cachedData = await _distributedCache.GetStringAsync(key);
            if (string.IsNullOrEmpty(cachedData))
                return default;
            
            return JsonSerializer.Deserialize<T>(cachedData);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expirationTime = null)
        {
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions();
            if (expirationTime.HasValue)
                options.SetAbsoluteExpiration(expirationTime.Value);

            string serializedData = JsonSerializer.Serialize(value);
            await _distributedCache.SetStringAsync(key, serializedData, options);
        }

        public async Task RemoveAsync(string key)
        {
            await _distributedCache.RemoveAsync(key);
        }

        public async Task RemoveByPrefixAsync(string prefix)
        {
            // IDistributedCache prefix bazlı silmeyi desteklemez. 
            // Bu yüzden versiyonlama (versioning) sistemini kullanıyoruz.
            // Eğer prefix 'catalog:' ise, versiyon anahtarını siliyoruz/güncelliyoruz.
            if (prefix.StartsWith("catalog"))
            {
                await _distributedCache.RemoveAsync("catalog:version");
            }
        }
    }
}
