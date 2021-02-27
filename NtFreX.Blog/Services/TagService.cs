using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Bson;
using MongoDB.Driver;
using NtFreX.Blog.Cache;
using NtFreX.Blog.Data;
using NtFreX.Blog.Model;

namespace NtFreX.Blog.Services
{
    public class TagService
    {
        private readonly IDistributedCache cache;
        private readonly IMongoCollection<TagModel> tags;
        private readonly IMongoCollection<ArticleModel> articles;

        public TagService(Database database, IDistributedCache cache)
        {
            tags = database.Blog.GetCollection<TagModel>("tag");
            articles = database.Blog.GetCollection<ArticleModel>("article");
            this.cache = cache;
        }

        public async Task<IReadOnlyList<string>> GetAllDistinctPublishedTagsAsync()
        {
            return await cache.CacheAsync(CacheKeys.AllDistinctPublishedTags, CacheKeys.TimeToLive, async () =>
            {
                var all = await GetAllTagsAsync();
                var allArticles = await articles.Find(_ => true).ToListAsync();
                return all.Where(d => ArticleService.IsPublished(allArticles.First(a => a.Id == d.ArticleId))).GroupBy(d => d.Name).OrderByDescending(d => d.Count()).Select(d => d.Key).ToList();
            });            
        }

        public async Task<IReadOnlyList<TagModel>> GetAllTagsAsync()
        {
            return await cache.CacheAsync(CacheKeys.AllTags, CacheKeys.TimeToLive, async () =>
            {
                return await tags.Find(_ => true).ToListAsync();
            });
        }

        public async Task<IReadOnlyList<TagModel>> GetTagsByArticleIdAsync(string id)
        {
            return await cache.CacheAsync(CacheKeys.TagsByArticleId(id), CacheKeys.TimeToLive, async () =>
            {
                var objectId = new ObjectId(id);
                return await tags.Find(d => d.ArticleId == objectId).ToListAsync();
            });
        }

        public async Task UpdateTagsForArticle(string[] newTags, ObjectId articleId)
        {
            var oldTags = await tags.Find(Builders<TagModel>.Filter.Eq(t => t.ArticleId, articleId)).ToListAsync();
            await tags.DeleteManyAsync(Builders<TagModel>.Filter.Eq(d => d.ArticleId, articleId));
            
            if (newTags.Any())
            {
                await tags.InsertManyAsync(newTags.Select(x => new TagModel { ArticleId = articleId, Name = x }));
            }

            await cache.RemoveAsync(CacheKeys.AllTags);
            await cache.RemoveAsync(CacheKeys.AllDistinctPublishedTags);
            await cache.RemoveAsync(CacheKeys.TagsByArticleId(articleId.ToString()));

            foreach (var tag in newTags.Concat(oldTags.Select(x => x.Name)))
            {
                await cache.RemoveAsync(CacheKeys.ArticlesByTag(tag));
            }
        }
    }
}
