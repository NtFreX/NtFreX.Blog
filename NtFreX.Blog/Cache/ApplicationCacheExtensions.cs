using System;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using StackExchange.Redis;

namespace NtFreX.Blog.Cache
{
    public static class ApplicationCacheExtensions
    {
        private static bool wasAvailable = false;
        private static DateTime lastAvailabilityCheck = DateTime.MinValue;

        // if a connection to redis fails then retry connecting for a new request after 5 minutes
        private static bool TryConnect() => wasAvailable || lastAvailabilityCheck < DateTime.Now - TimeSpan.FromMinutes(5);

        public static async Task RemoveSaveAsync(this ApplicationCache cache, string key)
        {

            if (TryConnect())
            {
                try
                {
                    await cache.RemoveAsync(key);
                    wasAvailable = true;
                }
                catch (RedisConnectionException)
                {
                    wasAvailable = false;
                }
                finally
                {
                    lastAvailabilityCheck = DateTime.Now;
                }
            }
        }
        public static async Task<T> CacheAsync<T>(this ApplicationCache cache, string key, TimeSpan livetime, Func<Task<T>> resolver)
        {
            if (TryConnect())
            {
                try
                {
                    var cached = await cache.GetAsync(key);
                    wasAvailable = true;
                    if (cached != null)
                    {
                        return BsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(cached));
                    }
                }
                catch (RedisConnectionException)
                {
                    wasAvailable = false;
                }
                finally
                {
                    lastAvailabilityCheck = DateTime.Now;
                }
            }

            var value = await resolver();

            if (TryConnect())
            {
                try
                {
                    await cache.SetAsync(key, Encoding.UTF8.GetBytes(value.ToJson()), livetime);
                    wasAvailable = true;
                }
                catch (RedisConnectionException)
                {
                    wasAvailable = false;
                }
                finally
                {
                    lastAvailabilityCheck = DateTime.Now;
                }
            }

            return value;
        }
    }
}
