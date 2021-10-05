using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Data
{
    public class MongoDbTagRepository : ITagRepository
    {
        private readonly IMongoCollection<TagModel> tags;
        
        public MongoDbTagRepository(MongoDatabase database)
        {
            tags = database.Blog.GetCollection<TagModel>("tag");
        }

        public async Task<IReadOnlyList<TagModel>> FindAsync()
           => await tags.Find(_ => true).ToListAsync();

        public async Task<IReadOnlyList<TagModel>> FindAsync(string articleId)
            => await tags.Find(d => d.ArticleId == articleId).ToListAsync();

        public async Task UpdateTagsForArticle(string[] newTags, string articleId)
        {
            await tags.DeleteManyAsync(Builders<TagModel>.Filter.Eq(d => d.ArticleId, articleId));

            if (newTags.Any())
            {
                await tags.InsertManyAsync(newTags.Select(x => new TagModel { ArticleId = articleId, Name = x }));
            }
        }
    }
}
