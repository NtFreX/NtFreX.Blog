using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Bson;
using MongoDB.Driver;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Data
{
    public class ArticleRepository
    {
        private readonly IMongoCollection<ArticleModel> article;

        public ArticleRepository(Database database)
        {
            article = database.Blog.GetCollection<ArticleModel>("article");
        }

        public async Task<string> InsertArticleAsync()
        {
            var model = new ArticleModel();
            await article.InsertOneAsync(model);
            return model.Id.ToString();
        }

        public async Task UpdateAsync(ArticleDto model)
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
        }

        public async Task<IReadOnlyList<ArticleDto>> FindAsync(bool includeUnpublished)
        {
            var items = await article.Find(_ => true).ToListAsync();
            return items.Select(x => x.ToDto()).Where(d => includeUnpublished || d.IsPublished()).OrderByDescending(d => d.Date).ToList();
        }

        public async Task<ArticleDto> FindByIdAsync(string id)
        {
            var objectId = new ObjectId(id);
            var dbModel = await article.Find(d => d.Id == objectId).FirstAsync();
            return dbModel.ToDto();
        }        
    }
}
