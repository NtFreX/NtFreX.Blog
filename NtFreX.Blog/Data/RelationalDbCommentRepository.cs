using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;
using MongoDB.Driver;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Data
{
    /*
    create table if not exists `comment` (
        `Id` varchar(255) not null unique, 
        `ArticleId` varchar(255) not null, 
        `Date` DATE not null, 
        `User` text,
        `Title` text, 
        `Content` text, 
        `AnalysisPrediction` boolean default null,
        `AnalysisProbability` float default null,
        `AnalysisSentiment` text,
        primary key ( `Id` ) 
    );
    */

    public class RelationalDbCommentRepository : ICommentRepository
    {
        private readonly MySqlDatabaseConnectionFactory connectionFactory;

        public RelationalDbCommentRepository(MySqlDatabaseConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        public async Task<IReadOnlyList<CommentModel>> GetCommentsByArticleIdAsync(string id)
            => (await connectionFactory.Connection.GetAllAsync<CommentModel>()).ToList().Where(x => x.ArticleId.ToString() == id).ToList();

        public async Task InsertCommentAsync(CommentModel model)
        {
            model.Id = Guid.NewGuid().ToString();
            await connectionFactory.Connection.InsertAsync(model);
        }
    }
}
