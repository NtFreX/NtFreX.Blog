using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Bson;
using MongoDB.Driver;
using NtFreX.Blog.Cache;
using NtFreX.Blog.Data;
using NtFreX.Blog.Model;

namespace NtFreX.Blog.Services
{
    public class CommentService
    {
        private readonly IDistributedCache cache;
        private readonly IMongoCollection<CommentModel> collection;

        public CommentService(Database database, IDistributedCache cache)
        {
            collection = database.Blog.GetCollection<CommentModel>("comment");
            this.cache = cache;
        }


        public async Task<IReadOnlyList<CommentModel>> GetCommentsByArticleIdAsync(string id)
        {
            return await cache.CacheAsync(CacheKeys.CommentsByArticleId(id), CacheKeys.TimeToLive, async () =>
            {
                var objectId = new ObjectId(id);
                return await collection.Find(d => d.ArticleId == objectId).ToListAsync();
            });            
        }

        public async Task InsertCommentAsync(CommentModel model)
        {
            model.Date = DateTime.UtcNow;
            await collection.InsertOneAsync(model);

            await cache.RemoveAsync(CacheKeys.CommentsByArticleId(model.ArticleId.ToString()));
        }
    }
}
