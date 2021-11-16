using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dapper.Contrib.Extensions;
using MongoDB.Driver;

namespace NtFreX.Blog.Data.EfCore
{
    public class RelationalDbRepository<TDbModel, TModel> : IRepository<TModel>
        where TDbModel : class, IEfCoreDbModel
    {
        private readonly MySqlDatabaseConnectionFactory connectionFactory;
        private readonly IMapper mapper;

        public RelationalDbRepository(MySqlDatabaseConnectionFactory connectionFactory, IMapper mapper)
        {
            this.connectionFactory = connectionFactory;
            this.mapper = mapper;
        }

        public async Task<IReadOnlyList<TModel>> FindAsync()
        {
            var dbModels = await connectionFactory.Connection.GetAllAsync<TDbModel>();
            return mapper.Map<List<TModel>>(dbModels.ToList());
        }

        public async Task<TModel> FindByIdAsync(string id)
        {
            var dbModel = await connectionFactory.Connection.GetAsync<TDbModel>(id);
            return mapper.Map<TModel>(dbModel);
        }

        public async Task<string> InsertAsync(TModel model)
        {
            var dbModel = mapper.Map<TDbModel>(model);
            dbModel.Id = Guid.NewGuid().ToString();

            await connectionFactory.Connection.InsertAsync(dbModel);
            return dbModel.Id.ToString();
        }

        public async Task UpdateAsync(TModel model)
        {
            var dbModel = mapper.Map<TDbModel>(model);
            await connectionFactory.Connection.UpdateAsync(dbModel);
        }

        public async Task InsertOrUpdate(TModel model)
        {
            var dbModel = mapper.Map<TDbModel>(model);
            if (!string.IsNullOrEmpty(dbModel.Id) && await connectionFactory.Connection.GetAsync<TDbModel>(dbModel.Id) != null)
                await UpdateAsync(model);
            else
                await InsertAsync(model);
        }
    }
}
