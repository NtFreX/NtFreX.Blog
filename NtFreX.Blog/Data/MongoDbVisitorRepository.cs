using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Data
{
    public class MongoDbVisitorRepository : VisitorRepository
    {
        private readonly IMongoCollection<VisitorModel> visitor;

        public MongoDbVisitorRepository(MongoDatabase database)
        {
            visitor = database.Blog.GetCollection<VisitorModel>("visitor");
        }

        public override async Task InsertAsync(VisitorModel model)
            => await visitor.InsertOneAsync(model);

        public override async Task<long> CountAsync(string id)
        {
            var items = await visitor.Find(Builders<VisitorModel>.Filter.Eq(d => d.Article, id)).ToListAsync();
            return items.Count(DoCountVisitor);
        }
    }
}
