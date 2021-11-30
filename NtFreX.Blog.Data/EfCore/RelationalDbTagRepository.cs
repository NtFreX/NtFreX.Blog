using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using Dapper.Contrib.Extensions;
using MongoDB.Driver;
using MySql.Data.MySqlClient;
using NtFreX.Blog.Configuration;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Data.EfCore
{
    public class RelationalDbTagRepository : RelationalDbRepository<Models.TagModel, TagModel>, ITagRepository
    {
        private readonly MySqlConnectionFactory connectionFactory;
        private readonly IMapper mapper;

        public RelationalDbTagRepository(MySqlConnectionFactory connectionFactory, IMapper mapper)
            : base(connectionFactory, mapper)
        {
            this.connectionFactory = connectionFactory;
            this.mapper = mapper;
        }

        public async Task<IReadOnlyList<TagModel>> FindByArticleIdAsync(string articleId)
        {
            var activitySource = new ActivitySource(BlogConfiguration.ActivitySourceName);
            using (var activity = activitySource.StartActivity($"{nameof(RelationalDbTagRepository)}.{nameof(FindByArticleIdAsync)}", ActivityKind.Server))
            {
                var dbModels = await connectionFactory.Connection.GetAllAsync<Models.TagModel>();
                return mapper.Map<List<TagModel>>(dbModels.Where(x => x.ArticleId == articleId).ToList());
            }
        }

        public async Task UpdateTagsForArticle(string[] newTags, string articleId)
        {
            var activitySource = new ActivitySource(BlogConfiguration.ActivitySourceName);
            using (var activity = activitySource.StartActivity($"{nameof(RelationalDbTagRepository)}.{nameof(UpdateTagsForArticle)}", ActivityKind.Server))
            {
                foreach (var tag in await FindByArticleIdAsync(articleId))
                {
                    await connectionFactory.Connection.DeleteAsync(mapper.Map<Models.TagModel>(tag), connectionFactory.Transaction);
                }

                foreach (var tag in newTags)
                {
                    await connectionFactory.Connection.InsertAsync(new Models.TagModel { ArticleId = articleId, Id = Guid.NewGuid().ToString(), Name = tag }, connectionFactory.Transaction);
                }
            }
        }

        public static void EnsureTableExists(MySqlConnection connection)
        {
            connection.Execute(@"
create table if not exists `tag` (
    `Id` varchar(255) not null unique, 
    `ArticleId` varchar(255) not null, 
    `Name` text, 
    primary key ( `Id` ) 
);");
        }
    }
}
