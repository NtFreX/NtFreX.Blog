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

        public VisitorRepository(Database database)
        {
            visitor = database.Blog.GetCollection<VisitorModel>("visitor");
        }

        public async Task InsertAsync(string id, string remoteIp, string userAgent)
        {
            var model = new VisitorModel
            {
                Date = DateTime.Now,
                RemoteIp = remoteIp,
                Article = id,
                UserAgent = userAgent
            };
            await visitor.InsertOneAsync(model);
        }

        public async Task<long> CountAsync(string id)
        {
            var items = await visitor.Find(Builders<VisitorModel>.Filter.Eq(d => d.Article, id)).ToListAsync();
            return items.Count(d => string.IsNullOrEmpty(d.RemoteIp) || !IPAddress.IsLoopback(IPAddress.Parse(d.RemoteIp)));
        }
    }
}
