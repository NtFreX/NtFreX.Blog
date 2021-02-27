using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace NtFreX.Blog.Cache
{
    public static class DistributedCacheExtensions
    {
        public static async Task<T> CacheAsync<T>(this IDistributedCache cache, string key, TimeSpan livetime, Func<Task<T>> resolver)
        {
            try
            {
                var cached = await cache.GetAsync(key);
                if (cached != null)
                {
                    return BsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(cached));
                }
            }
            catch { }

            var value = await resolver();

            try
            {
                var options = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(livetime);

                await cache.SetAsync(key, Encoding.UTF8.GetBytes(value.ToJson()), options);
            }
            catch { }

            return value;
        }
    }
}
