using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Bson;
using MongoDB.Driver;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Data
{
    public class MongoDbArticleRepository : IArticleRepository
    {
        private readonly IMongoCollection<ArticleModel> article;

        public MongoDbArticleRepository(MongoDatabase database)
        {
            article = database.Blog.GetCollection<ArticleModel>("article");
        }

        public async Task<string> InsertArticleAsync()
        {
            var model = new ArticleModel();
            await article.InsertOneAsync(model);
            return model.Id.ToString();
        }

        public async Task UpdateAsync(ArticleModel model)
        {
            await article.UpdateOneAsync(
                Builders<ArticleModel>.Filter.Eq(d => d.Id, model.Id),
                Builders<ArticleModel>.Update
                    .Set(d => d.Title, model.Title)
                    .Set(d => d.Subtitle, model.Subtitle)
                    .Set(d => d.Date, model.Date)
                    .Set(d => d.Published, model.Published)
                    .Set(d => d.Content, model.Content));
        }

        public async Task<IReadOnlyList<ArticleModel>> FindAsync(bool includeUnpublished)
        {
            var items = await article.Find(_ => true).ToListAsync();
            return items.Where(d => includeUnpublished || d.ToDto().IsPublished()).OrderByDescending(d => d.Date).ToList();
        }

        public async Task<ArticleModel> FindByIdAsync(string id)
            => await article.Find(d => d.Id == id).FirstAsync();
    }
}
