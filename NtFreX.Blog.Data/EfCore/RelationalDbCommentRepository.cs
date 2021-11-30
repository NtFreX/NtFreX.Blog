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
    public class RelationalDbCommentRepository : RelationalDbRepository<Models.CommentModel, CommentModel>, ICommentRepository
    {
        private readonly MySqlConnectionFactory connectionFactory;
        private readonly IMapper mapper;

        public RelationalDbCommentRepository(MySqlConnectionFactory connectionFactory, IMapper mapper)
            : base(connectionFactory, mapper)
        {
            this.connectionFactory = connectionFactory;
            this.mapper = mapper;
        }

        public async Task<IReadOnlyList<CommentModel>> FindByArticleIdAsync(string id)
        {
            var activitySource = new ActivitySource(BlogConfiguration.ActivitySourceName);
            using (var activity = activitySource.StartActivity($"{nameof(RelationalDbCommentRepository)}.{nameof(FindByArticleIdAsync)}", ActivityKind.Server))
            {
                var dbModels = await connectionFactory.Connection.GetAllAsync<Models.CommentModel>();
                return mapper.Map<List<CommentModel>>(dbModels.Where(x => x.ArticleId.ToString() == id).ToList());
            }
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
