using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Data
{
    public class MongoDbCommentRepository : ICommentRepository
    {
        private readonly IMongoCollection<CommentModel> collection;

        public MongoDbCommentRepository(MongoDatabase database)
        {
            collection = database.Blog.GetCollection<CommentModel>("comment");
        }

        public async Task<IReadOnlyList<CommentModel>> GetCommentsByArticleIdAsync(string id)
            => await collection.Find(d => d.ArticleId == id).ToListAsync();

        public async Task InsertCommentAsync(CommentModel model)
            => await collection.InsertOneAsync(model);
    }
}
