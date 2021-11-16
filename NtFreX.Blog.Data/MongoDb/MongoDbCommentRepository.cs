using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using MongoDB.Driver;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Data.MongoDb
{
    public class MongoDbCommentRepository : MongoDbRepository<Models.CommentModel, CommentModel>, ICommentRepository
    {
        private readonly IMapper mapper;

        public MongoDbCommentRepository(MongoDatabase database, IMapper mapper)
            : base(database.Blog.GetCollection<Models.CommentModel>("comment"), mapper)
        {
            this.mapper = mapper;
        }

        public async Task<IReadOnlyList<CommentModel>> FindByArticleIdAsync(string id)
        {
            var dbModels = await Collection.Find(d => d.ArticleId == id).ToListAsync();
            return mapper.Map<List<CommentModel>>(dbModels);
        }

        public override Task UpdateAsync(CommentModel model)
            => throw new System.NotSupportedException("Updating a comment is not supported");
    }
}
