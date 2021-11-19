using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NtFreX.Blog.Configuration;
using NtFreX.Blog.Logging;

namespace NtFreX.Blog.Cache
{
    public class ApplicationCache
    {
        private readonly TraceActivityDecorator traceActivityDecorator;
        private readonly IMemoryCache memoryCache;
        private readonly IDistributedCache distributedCache;
        private readonly ILogger<ApplicationCache> logger;

        public ApplicationCache(TraceActivityDecorator traceActivityDecorator, IMemoryCache memoryCache, IDistributedCache distributedCache, ILogger<ApplicationCache> logger)
        {
            this.traceActivityDecorator = traceActivityDecorator;
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
            using (var activity = activitySource.StartActivity($"{nameof(ApplicationCache)}.{nameof(GetAsync)}", ActivityKind.Server))
            {
                logger.LogTrace($"TraceId={activity.TraceId}: Requesting cache for key {key}");

                traceActivityDecorator.Decorate(activity);
                activity.AddTag("cacheKey", key);

                var cacheResult = BlogConfiguration.ApplicationCacheType switch
                {
                    CacheType.InMemory => memoryCache.Get<byte[]>(key),
                    CacheType.Distributed => await distributedCache.GetAsync(key, token),
                    _ => throw new ArgumentException($"The given cache type '{BlogConfiguration.ApplicationCacheType}' is not known")
                };

                var meter = new Meter(BlogConfiguration.MetricsName);
                meter.CreateObservableGauge(
                    $"CacheHit", 
                    () => new Measurement<int>(
                        cacheResult == null ? 0 : 1, 
                        new KeyValuePair<string, object>("key", key),
                        new KeyValuePair<string, object>("machine", System.Environment.MachineName),
                        new KeyValuePair<string, object>("type", BlogConfiguration.ApplicationCacheType.ToString())), 
                    "0 = cache has not been hit, 1 = cache has been hit");

                logger.LogDebug($"Hitting cache for key {key} {(cacheResult == null ? "didn't" : "did")} return a value");

                return cacheResult;
            }
        }

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            var activitySource = new ActivitySource(BlogConfiguration.ActivitySourceName);
            using (var activity = activitySource.StartActivity($"{nameof(ApplicationCache)}.{nameof(GetAsync)}", ActivityKind.Server))
            {
                traceActivityDecorator.Decorate(activity);
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
}
