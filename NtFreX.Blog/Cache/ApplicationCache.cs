using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NtFreX.Blog.Configuration;

namespace NtFreX.Blog.Cache
{
    public class ApplicationCache
    {
        private readonly ApplicationContextActivityDecorator applicationContextActivityDecorator;
        private readonly IMemoryCache memoryCache;
        private readonly IDistributedCache distributedCache;
        private readonly ILogger<ApplicationCache> logger;

        private static readonly Counter<int> CacheHitCounter = Program.Meter.CreateCounter<int>($"CacheHits", description: "The number of cache hits");
        private static readonly Counter<int> CacheHitSuccessCounter = Program.Meter.CreateCounter<int>($"CacheHitSuccesses", description: "The number of successful cache hits");
        private static readonly Counter<int> CacheHitFailedCounter = Program.Meter.CreateCounter<int>($"CacheHitFailures", description: "The number of failed cache hits");

        public ApplicationCache(ApplicationContextActivityDecorator applicationContextActivityDecorator, IMemoryCache memoryCache, IDistributedCache distributedCache, ILogger<ApplicationCache> logger)
        {
            this.applicationContextActivityDecorator = applicationContextActivityDecorator;
            this.memoryCache = memoryCache;
            this.distributedCache = distributedCache;
            this.logger = logger;
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
            using var activity = applicationContextActivityDecorator.StartActivity();
            activity.AddTag("cacheKey", key);

            logger.LogTrace($"Requesting cache for key {key}");

            var cacheResult = BlogConfiguration.ApplicationCacheType switch
            {
                CacheType.InMemory => memoryCache.Get<byte[]>(key),
                CacheType.Distributed => await distributedCache.GetAsync(key, token),
                _ => throw new ArgumentException($"The given cache type '{BlogConfiguration.ApplicationCacheType}' is not known")
            };

            var tags = new[] {
                new KeyValuePair<string, object>("key", key),                
                new KeyValuePair<string, object>("type", BlogConfiguration.ApplicationCacheType.ToString())
            }.Concat(MetricTags.GetDefaultTags()).ToArray();

            CacheHitCounter.Add(1, tags);
            CacheHitSuccessCounter.Add(cacheResult == null ? 0 : 1, tags);
            CacheHitFailedCounter.Add(cacheResult == null ? 1 : 0, tags);

            logger.LogDebug($"Hitting cache for key {key} {(cacheResult == null ? "didn't" : "did")} return a value");

            return cacheResult;
        }

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            using var activity = applicationContextActivityDecorator.StartActivity();
            activity.AddTag("cacheKey", key);

            logger.LogInformation($"Removing cached value for key {key}");

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
