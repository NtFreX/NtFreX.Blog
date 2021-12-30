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
    public class RelationalDbCommentRepository : RelationalDbRepository<Models.CommentModel, CommentModel>, ICommentRepository
    {
        private readonly MySqlConnectionFactory connectionFactory;
        private readonly IMapper mapper;
        private readonly ApplicationContextActivityDecorator applicationContextActivityDecorator;

        public RelationalDbCommentRepository(MySqlConnectionFactory connectionFactory, IMapper mapper, ApplicationContextActivityDecorator applicationContextActivityDecorator)
            : base(connectionFactory, mapper, applicationContextActivityDecorator)
        {
            this.connectionFactory = connectionFactory;
            this.mapper = mapper;
            this.applicationContextActivityDecorator = applicationContextActivityDecorator;
        }

        public async Task<IReadOnlyList<CommentModel>> FindByArticleIdAsync(string id)
        {
            var activity = applicationContextActivityDecorator.StartActivity($"{nameof(RelationalDbCommentRepository)}.{nameof(FindByArticleIdAsync)}");
            var dbModels = await connectionFactory.Connection.GetAllAsync<Models.CommentModel>();
            return mapper.Map<List<CommentModel>>(dbModels.Where(x => x.ArticleId.ToString() == id).ToList());
        }

        public static void EnsureTableExists(MySqlConnection connection)
        {
            connection.Execute(@"
create table if not exists `comment` (
    `Id` varchar(255) not null unique, 
    `ArticleId` varchar(255) not null, 
    `Date` DATETIME not null, 
    `User` text,
    `Title` text, 
    `Content` text, 
    `AnalysisPrediction` boolean default null,
    `AnalysisProbability` float default null,
    `AnalysisSentiment` text,
    primary key ( `Id` ) 
);");
        }
    }
}
