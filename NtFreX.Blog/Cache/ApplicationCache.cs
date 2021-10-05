using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using NtFreX.Blog.Configuration;

namespace NtFreX.Blog.Cache
{
    public class ApplicationCache
    {
        private readonly IMemoryCache memoryCache;
        private readonly IDistributedCache distributedCache;

        public ApplicationCache(IMemoryCache memoryCache, IDistributedCache distributedCache)
        {
            this.memoryCache = memoryCache;
            this.distributedCache = distributedCache;
        }

        public async Task SetAsync(string key, byte[] value, TimeSpan absoluteExpiration, CancellationToken token = default)
        {
            switch (BlogConfiguration.ApplicationCacheType)
            {
                case CacheType.InMemory:
                    memoryCache.Set(key, value, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = absoluteExpiration });
                    break;
                case CacheType.Distributed:
                    await distributedCache.SetAsync(key, value, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = absoluteExpiration }, token);
                    break;
                default:
                    throw new ArgumentException($"The given cache type '{BlogConfiguration.ApplicationCacheType}' is not known");
            }
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            return BlogConfiguration.ApplicationCacheType switch
            {
                CacheType.InMemory => memoryCache.Get<byte[]>(key),
                CacheType.Distributed => await distributedCache.GetAsync(key, token),
                _ => throw new ArgumentException($"The given cache type '{BlogConfiguration.ApplicationCacheType}' is not known")
            };
        }

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            switch (BlogConfiguration.ApplicationCacheType)
            {
                case CacheType.InMemory:
                    memoryCache.Remove(key);
                    break;
                case CacheType.Distributed:
                    await distributedCache.RemoveAsync(key, token);
                    break;
                default:
                    throw new ArgumentException($"The given cache type '{BlogConfiguration.ApplicationCacheType}' is not known");
            }
        }
    }
}
