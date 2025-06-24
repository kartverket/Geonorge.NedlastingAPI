using Microsoft.Extensions.Caching.Memory;

namespace Geonorge.Download.Services.Misc
{
    public class MemoryCacher(MemoryCache memoryCache)
    {
        public object GetValue(string key)
        {
            memoryCache.TryGetValue(key, out var value);
            return value;
        }

        public bool Add(string key, object value, DateTimeOffset absExpiration)
        {
            // Set absolute expiration
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = absExpiration
            };

            // Avoid overwriting existing keys (like MemoryCache.Add in .NET Framework)
            if (!memoryCache.TryGetValue(key, out _))
            {
                memoryCache.Set(key, value, cacheEntryOptions);
                return true;
            }

            return false;
        }

        public void Delete(string key)
        {
            memoryCache.Remove(key);
        }
    }
}
