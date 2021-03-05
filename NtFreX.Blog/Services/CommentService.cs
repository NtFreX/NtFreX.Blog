using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Bson;
using MongoDB.Driver;
using NtFreX.Blog.Cache;
using NtFreX.Blog.Data;
using NtFreX.Blog.Models;

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


        public async Task<IReadOnlyList<CommentDto>> GetCommentsByArticleIdAsync(string id)
        {
            return await cache.CacheAsync(CacheKeys.CommentsByArticleId(id), CacheKeys.TimeToLive, async () =>
            {
                var objectId = new ObjectId(id);
                var dbModels = await collection.Find(d => d.ArticleId == objectId).ToListAsync();
                return dbModels.Select(x => x.ToDto()).ToList();
            });            
        }

        public async Task InsertCommentAsync(CreateCommentDto model)
        {
            var dbModel = new CommentModel
            {
                Date = DateTime.UtcNow,
                Content = model.Content,
                ArticleId = new ObjectId(model.ArticleId),
                Title = model.Title,
                User = model.User
            };
            await collection.InsertOneAsync(dbModel);

            await cache.RemoveAsync(CacheKeys.CommentsByArticleId(model.ArticleId.ToString()));
        }
    }
}
