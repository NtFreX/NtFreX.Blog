using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Bson;
using MongoDB.Driver;
using NtFreX.Blog.Cache;
using NtFreX.Blog.Data;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Data
{
    public class TagRepository
    {
        private readonly IMongoCollection<TagModel> tags;
        private readonly IDistributedCache cache; 
        
        public TagRepository(Database database, IDistributedCache cache)
        {
            tags = database.Blog.GetCollection<TagModel>("tag");
            this.cache = cache;
        }

        public async Task<IReadOnlyList<TagDto>> GetAllTagsAsync()
        {
            return await cache.CacheAsync(CacheKeys.AllTags, CacheKeys.TimeToLive, async () =>
            {
                var dbModels = await tags.Find(_ => true).ToListAsync();
                return dbModels.Select(x => x.ToDto()).ToList();
            });
        }

        public async Task<IReadOnlyList<TagDto>> GetTagsByArticleIdAsync(string id)
        {
            return await cache.CacheAsync(CacheKeys.TagsByArticleId(id), CacheKeys.TimeToLive, async () =>
            {
                var objectId = new ObjectId(id);
                var dbModels = await tags.Find(d => d.ArticleId == objectId).ToListAsync();
                return dbModels.Select(x => x.ToDto()).ToList();
            });
        }

        public async Task UpdateTagsForArticle([FromBody] string[] newTags, string articleId)
        {
            var objectId = new ObjectId(articleId);
            await tags.DeleteManyAsync(Builders<TagModel>.Filter.Eq(d => d.ArticleId, objectId));

            if (newTags.Any())
            {
                await tags.InsertManyAsync(newTags.Select(x => new TagModel { ArticleId = objectId, Name = x }));
            }

            await cache.RemoveAsync(CacheKeys.AllTags);
            await cache.RemoveAsync(CacheKeys.TagsByArticleId(articleId.ToString()));
        }
    }
}
