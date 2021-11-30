using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using MongoDB.Bson;
using MongoDB.Driver;

namespace NtFreX.Blog.Data.MongoDb
{
    public abstract class MongoDbRepository<TDbModel, TModel> : IRepository<TModel>
        where TDbModel : class, IMongoDbModel
    {
        protected readonly IMongoCollection<TDbModel> Collection;
        protected readonly MongoConnectionFactory Database;

        private readonly IMapper mapper;

        public MongoDbRepository(MongoConnectionFactory database, IMongoCollection<TDbModel> collection, IMapper mapper)
        {
            Database = database;
            this.Collection = collection;
            this.mapper = mapper;
        }


        public async Task<IReadOnlyList<TModel>> FindAsync()
        {
            var dbModels = await Collection.Find(_ => true).ToListAsync();
            return mapper.Map<List<TModel>>(dbModels);
        }

        public async Task<TModel> FindByIdAsync(string id)
        {
            var objectId = new ObjectId(id);
            var dbModel = await Collection.Find(x => x.Id == objectId).FirstAsync();
            return mapper.Map<TModel>(dbModel);
        }

        public async Task<string> InsertAsync(TModel model)
        {
            var dbModel = mapper.Map<TDbModel>(model);
            dbModel.Id = new ObjectId(Guid.NewGuid().ToString());

            await Collection.InsertOneAsync(Database.Session, dbModel);
            return dbModel.Id.ToString();
        }

        public async Task InsertOrUpdate(TModel model)
        {
            var dbModel = mapper.Map<TDbModel>(model);
            if (dbModel.Id == default)
                await InsertAsync(model);
            else
                await UpdateAsync(model);
        }

        public abstract Task UpdateAsync(TModel model);
    }
}
