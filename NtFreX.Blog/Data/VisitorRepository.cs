using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Driver;
using NtFreX.Blog.Cache;
using NtFreX.Blog.Data;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Data
{
    public class VisitorRepository
    {
        private readonly IMongoCollection<VisitorModel> visitor;
        private readonly IDistributedCache cache;

        public VisitorRepository(Database database, IDistributedCache cache)
        {
            visitor = database.Blog.GetCollection<VisitorModel>("visitor");
            this.cache = cache;
        }

        public async Task VisitArticleAsync(string id, string remoteIp, string userAgent)
        {
            var model = new VisitorModel
            {
                Date = DateTime.Now,
                RemoteIp = remoteIp,
                Article = id,
                UserAgent = userAgent
            };
            await visitor.InsertOneAsync(model);

            await cache.RemoveAsync(CacheKeys.VisitorsByArticleId(id));
        }

        public async Task<long> CountVisitorsAsync(string id)
        {
            return await cache.CacheAsync(CacheKeys.VisitorsByArticleId(id), CacheKeys.TimeToLive, async () =>
            {
                var items = await visitor.Find(Builders<VisitorModel>.Filter.Eq(d => d.Article, id)).ToListAsync();
                return items.Count(d => string.IsNullOrEmpty(d.RemoteIp) || !IPAddress.IsLoopback(IPAddress.Parse(d.RemoteIp)));
            });
        }
    }
}
