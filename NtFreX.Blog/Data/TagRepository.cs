using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Data
{
    public class TagRepository
    {
        private readonly IMongoCollection<TagModel> tags;
        
        public TagRepository(Database database)
        {
            tags = database.Blog.GetCollection<TagModel>("tag");
        }

        public async Task<IReadOnlyList<TagDto>> FindAsync()
        {
            var dbModels = await tags.Find(_ => true).ToListAsync();
            return dbModels.Select(x => x.ToDto()).ToList();
        }

        public async Task<IReadOnlyList<TagDto>> FindAsync(string articleId)
        {
            var objectId = new ObjectId(articleId);
            var dbModels = await tags.Find(d => d.ArticleId == objectId).ToListAsync();
            return dbModels.Select(x => x.ToDto()).ToList();
        }

        public async Task UpdateTagsForArticle(string[] newTags, string articleId)
        {
            var objectId = new ObjectId(articleId);
            await tags.DeleteManyAsync(Builders<TagModel>.Filter.Eq(d => d.ArticleId, objectId));

            if (newTags.Any())
            {
                await tags.InsertManyAsync(newTags.Select(x => new TagModel { ArticleId = objectId, Name = x }));
            }
        }
    }
}
