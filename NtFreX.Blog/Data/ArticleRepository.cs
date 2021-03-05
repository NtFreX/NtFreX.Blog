using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Bson;
using MongoDB.Driver;
using NtFreX.Blog.Cache;
using NtFreX.Blog.Data;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Data
{
    public class ArticleRepository
    {
        private readonly IMongoCollection<ArticleModel> article;
        private readonly IDistributedCache cache;

        public ArticleRepository(Database database, IDistributedCache cache)
        {
            article = database.Blog.GetCollection<ArticleModel>("article");
            this.cache = cache;
        }

        public async Task<string> CreateArticleAsync()
        {
            var model = new ArticleModel();
            await article.InsertOneAsync(model);

            await cache.RemoveAsync(CacheKeys.AllArticles);
            await cache.RemoveAsync(CacheKeys.AllPublishedArticles);

            return model.Id.ToString();
        }

        public async Task<IReadOnlyList<ArticleDto>> GetAllArticlesAsync(bool includeUnpublished)
        {
            return await cache.CacheAsync(includeUnpublished ? CacheKeys.AllArticles : CacheKeys.AllPublishedArticles, CacheKeys.TimeToLive, async () =>
            {
                var items = await article.Find(_ => true).ToListAsync();
                return items.Select(x => x.ToDto()).Where(d => includeUnpublished || d.IsPublished()).OrderByDescending(d => d.Date).ToList();
            });
        }

        public async Task<ArticleDto> GetArticleByIdAsync(string id)
        {
            return await cache.CacheAsync(CacheKeys.Article(id), CacheKeys.TimeToLive, async () =>
            {
                var objectId = new ObjectId(id);
                var dbModel = await article.Find(d => d.Id == objectId).FirstAsync();
                return dbModel.ToDto();
            });
        }

        public async Task SaveArticleAsync(ArticleDto model)
        {
            var articleId = new ObjectId(model.Id);

            await article.UpdateOneAsync(
                Builders<ArticleModel>.Filter.Eq(d => d.Id, articleId),
                Builders<ArticleModel>.Update
                    .Set(d => d.Title, model.Title)
                    .Set(d => d.Subtitle, model.Subtitle)
                    .Set(d => d.Date, model.Date)
                    .Set(d => d.Published, model.Published)
                    .Set(d => d.Content, model.Content));

            await cache.RemoveAsync(CacheKeys.Article(model.Id));
            await cache.RemoveAsync(CacheKeys.AllArticles);
            await cache.RemoveAsync(CacheKeys.AllPublishedArticles);
        }
    }
}
