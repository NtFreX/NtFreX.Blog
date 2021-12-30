using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dapper.Contrib.Extensions;
using MongoDB.Driver;
using NtFreX.Blog.Configuration;

namespace NtFreX.Blog.Data.EfCore
{
    public class RelationalDbRepository<TDbModel, TModel> : IRepository<TModel>
        where TDbModel : class, IEfCoreDbModel
    {
        private readonly MySqlConnectionFactory connectionFactory;
        private readonly IMapper mapper;
        private readonly ApplicationContextActivityDecorator applicationContextActivityDecorator;

        public RelationalDbRepository(MySqlConnectionFactory connectionFactory, IMapper mapper, ApplicationContextActivityDecorator applicationContextActivityDecorator)
        {
            this.connectionFactory = connectionFactory;
            this.mapper = mapper;
            this.applicationContextActivityDecorator = applicationContextActivityDecorator;
        }

        public async Task<IReadOnlyList<TModel>> FindAsync()
        {
            var activity = applicationContextActivityDecorator.StartActivity($"{nameof(RelationalDbRepository<TDbModel, TModel>)}.{nameof(FindAsync)}");
            var dbModels = await connectionFactory.Connection.GetAllAsync<TDbModel>();
            return mapper.Map<List<TModel>>(dbModels.ToList());
        }

        public async Task<TModel> FindByIdAsync(string id)
        {
            var activity = applicationContextActivityDecorator.StartActivity($"{nameof(RelationalDbRepository<TDbModel, TModel>)}.{nameof(FindByIdAsync)}");
            var dbModel = await connectionFactory.Connection.GetAsync<TDbModel>(id);
            return mapper.Map<TModel>(dbModel);
        }

        public async Task<string> InsertAsync(TModel model)
        {
            var activity = applicationContextActivityDecorator.StartActivity($"{nameof(RelationalDbRepository<TDbModel, TModel>)}.{nameof(InsertAsync)}");
            var dbModel = mapper.Map<TDbModel>(model);
            dbModel.Id = Guid.NewGuid().ToString();

            await connectionFactory.Connection.InsertAsync(dbModel, connectionFactory.Transaction);
            return dbModel.Id.ToString();
        }

        public async Task UpdateAsync(TModel model)
        {
            var activity = applicationContextActivityDecorator.StartActivity($"{nameof(RelationalDbRepository<TDbModel, TModel>)}.{nameof(UpdateAsync)}");
            var dbModel = mapper.Map<TDbModel>(model);
            await connectionFactory.Connection.UpdateAsync(dbModel, connectionFactory.Transaction);

        }

        public async Task InsertOrUpdate(TModel model)
        {
            var activity = applicationContextActivityDecorator.StartActivity($"{nameof(RelationalDbRepository<TDbModel, TModel>)}.{nameof(InsertOrUpdate)}");
            var dbModel = mapper.Map<TDbModel>(model);
            if (!string.IsNullOrEmpty(dbModel.Id) && await connectionFactory.Connection.GetAsync<TDbModel>(dbModel.Id) != null)
                await UpdateAsync(model);
            else
                await InsertAsync(model);
        }
    }
}
