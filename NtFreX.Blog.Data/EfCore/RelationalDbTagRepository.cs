using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using Dapper.Contrib.Extensions;
using MongoDB.Driver;
using MySql.Data.MySqlClient;
using NtFreX.Blog.Configuration;
using NtFreX.Blog.Data.Models;

namespace NtFreX.Blog.Data.EfCore
{
    public class RelationalDbTagRepository : RelationalDbRepository<Models.TagModel, TagModel>, ITagRepository
    {
        private readonly MySqlConnectionFactory connectionFactory;
        private readonly IMapper mapper;
        private readonly ApplicationContextActivityDecorator applicationContextActivityDecorator;

        public RelationalDbTagRepository(MySqlConnectionFactory connectionFactory, IMapper mapper, ApplicationContextActivityDecorator applicationContextActivityDecorator)
            : base(connectionFactory, mapper, applicationContextActivityDecorator)
        {
            this.connectionFactory = connectionFactory;
            this.mapper = mapper;
            this.applicationContextActivityDecorator = applicationContextActivityDecorator;
        }

        public async Task<IReadOnlyList<TagModel>> FindByArticleIdAsync(string articleId)
        {
            var activity = applicationContextActivityDecorator.StartActivity($"{nameof(RelationalDbTagRepository)}.{nameof(FindByArticleIdAsync)}");
            var dbModels = await connectionFactory.Connection.GetAllAsync<Models.TagModel>();
            return mapper.Map<List<TagModel>>(dbModels.Where(x => x.ArticleId == articleId).ToList());
        }

        public async Task UpdateTagsForArticle(string[] newTags, string articleId)
        {
            var activity = applicationContextActivityDecorator.StartActivity($"{nameof(RelationalDbTagRepository)}.{nameof(UpdateTagsForArticle)}");
            foreach (var tag in await FindByArticleIdAsync(articleId))
            {
                await connectionFactory.Connection.DeleteAsync(mapper.Map<Models.TagModel>(tag), connectionFactory.Transaction);
            }

            foreach (var tag in newTags)
            {
                await connectionFactory.Connection.InsertAsync(new Models.TagModel { ArticleId = articleId, Id = Guid.NewGuid().ToString(), Name = tag }, connectionFactory.Transaction);
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
