using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
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
        private readonly IMemoryCache memoryCache;
        private readonly IDistributedCache distributedCache;
        private readonly ILogger<ApplicationCache> logger;

        public ApplicationCache(IMemoryCache memoryCache, IDistributedCache distributedCache, ILogger<ApplicationCache> logger)
        {
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
            var activitySource = new ActivitySource(BlogConfiguration.ActivitySourceName);
            using (var sampleActivity = activitySource.StartActivity($"{nameof(ApplicationCache)}.{nameof(GetAsync)}", ActivityKind.Server))
            {
                logger.LogTrace($"TraceId={sampleActivity.TraceId}: Requesting cache for key {key}");

                sampleActivity.AddBaggage("Environment.MachineName", System.Environment.MachineName);
                sampleActivity.AddTag("CacheKey", key);

                var cacheResult = BlogConfiguration.ApplicationCacheType switch
                {
                    CacheType.InMemory => memoryCache.Get<byte[]>(key),
                    CacheType.Distributed => await distributedCache.GetAsync(key, token),
                    _ => throw new ArgumentException($"The given cache type '{BlogConfiguration.ApplicationCacheType}' is not known")
                };

                var meter = new Meter(BlogConfiguration.MetricsName);
                meter.CreateObservableGauge($"CacheHit.{key}", () => new[] { new Measurement<int>(cacheResult == null ? 0 : 1) }, "0 = cache has not been hit, 1 = cache has been hit");

                logger.LogDebug($"TraceId={sampleActivity.TraceId}: Hitting cache for key {key} {(cacheResult == null ? "didn't" : "did")} return a value");

                return cacheResult;
            }
        }

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            var activitySource = new ActivitySource(BlogConfiguration.ActivitySourceName);
            using (var sampleActivity = activitySource.StartActivity($"{nameof(ApplicationCache)}.{nameof(GetAsync)}", ActivityKind.Server))
            {
                logger.LogInformation($"TraceId={sampleActivity.TraceId}: Removing cached value for key {key}");

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
}
