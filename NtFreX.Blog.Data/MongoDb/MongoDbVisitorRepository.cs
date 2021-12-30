using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MongoDB.Driver;
using NtFreX.Blog.Data.Models;

namespace NtFreX.Blog.Data.MongoDb
{
    public class MongoDbVisitorRepository : MongoDbRepository<Models.VisitorModel, VisitorModel>, IVisitorRepository
    {
        private readonly IMapper mapper;

        public MongoDbVisitorRepository(MongoConnectionFactory database, IMapper mapper)
            : base(database, database.Blog.GetCollection<Models.VisitorModel>("visitor"), mapper)
        {
            this.mapper = mapper;
        }

        public async Task<long> CountByArticleIdAsync(string id)
        {
            var dbModels = await Collection.Find(Builders<Models.VisitorModel>.Filter.Eq(d => d.Article, id)).ToListAsync();
            var models = mapper.Map<List<VisitorModel>>(dbModels);
            return models.Count(IVisitorRepository.ShouldCountVisitor);
        }

        public override Task UpdateAsync(VisitorModel model)
            => throw new NotSupportedException("Updating a visitor is not supported");
    }
}
