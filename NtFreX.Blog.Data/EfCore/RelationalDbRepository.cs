using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public RelationalDbRepository(MySqlConnectionFactory connectionFactory, IMapper mapper)
        {
            this.connectionFactory = connectionFactory;
            this.mapper = mapper;
        }

        public async Task<IReadOnlyList<TModel>> FindAsync()
        {
            var activitySource = new ActivitySource(BlogConfiguration.ActivitySourceName);
            using (var activity = activitySource.StartActivity($"{nameof(RelationalDbRepository<TDbModel, TModel>)}.{nameof(FindAsync)}", ActivityKind.Server))
            {
                var dbModels = await connectionFactory.Connection.GetAllAsync<TDbModel>();
                return mapper.Map<List<TModel>>(dbModels.ToList());
            }
        }

        public async Task<TModel> FindByIdAsync(string id)
        {
            var activitySource = new ActivitySource(BlogConfiguration.ActivitySourceName);
            using (var activity = activitySource.StartActivity($"{nameof(RelationalDbRepository<TDbModel, TModel>)}.{nameof(FindByIdAsync)}", ActivityKind.Server))
            {
                var dbModel = await connectionFactory.Connection.GetAsync<TDbModel>(id);
                return mapper.Map<TModel>(dbModel);
            }
        }

        public async Task<string> InsertAsync(TModel model)
        {
            var activitySource = new ActivitySource(BlogConfiguration.ActivitySourceName);
            using (var activity = activitySource.StartActivity($"{nameof(RelationalDbRepository<TDbModel, TModel>)}.{nameof(InsertAsync)}", ActivityKind.Server))
            {
                var dbModel = mapper.Map<TDbModel>(model);
                dbModel.Id = Guid.NewGuid().ToString();

                await connectionFactory.Connection.InsertAsync(dbModel, connectionFactory.Transaction);
                return dbModel.Id.ToString();
            }
        }

        public async Task UpdateAsync(TModel model)
        {
            var activitySource = new ActivitySource(BlogConfiguration.ActivitySourceName);
            using (var activity = activitySource.StartActivity($"{nameof(RelationalDbRepository<TDbModel, TModel>)}.{nameof(UpdateAsync)}", ActivityKind.Server))
            {
                var dbModel = mapper.Map<TDbModel>(model);
                await connectionFactory.Connection.UpdateAsync(dbModel, connectionFactory.Transaction);
            }
        }

        public async Task InsertOrUpdate(TModel model)
        {
            var activitySource = new ActivitySource(BlogConfiguration.ActivitySourceName);
            using (var activity = activitySource.StartActivity($"{nameof(RelationalDbRepository<TDbModel, TModel>)}.{nameof(InsertOrUpdate)}", ActivityKind.Server))
            {
                var dbModel = mapper.Map<TDbModel>(model);
                if (!string.IsNullOrEmpty(dbModel.Id) && await connectionFactory.Connection.GetAsync<TDbModel>(dbModel.Id) != null)
                    await UpdateAsync(model);
                else
                    await InsertAsync(model);
            }
        }
    }
}
