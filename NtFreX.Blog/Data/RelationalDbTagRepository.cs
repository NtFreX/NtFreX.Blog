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
     
    create table if not exists `tag` (
        `Id` varchar(255) not null unique, 
        `ArticleId` varchar(255) not null, 
        `Name` text, 
        primary key ( `Id` ) 
    );

    */

    public class RelationalDbTagRepository : ITagRepository
    {
        private readonly MySqlDatabaseConnectionFactory connectionFactory;

        public RelationalDbTagRepository(MySqlDatabaseConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        public async Task<IReadOnlyList<TagModel>> FindAsync()
            => (await connectionFactory.Connection.GetAllAsync<TagModel>()).ToList();

        public async Task<IReadOnlyList<TagModel>> FindAsync(string articleId)
            => (await FindAsync()).Where(x => x.ArticleId == articleId).ToList();

        public async Task UpdateTagsForArticle(string[] newTags, string articleId)
        {
            foreach (var tag in await FindAsync(articleId)) 
            {
                await connectionFactory.Connection.DeleteAsync(tag);
            }

            foreach(var tag in newTags)
            {
                await connectionFactory.Connection.InsertAsync(new TagModel { ArticleId = articleId, Id = Guid.NewGuid().ToString(), Name = tag });
            }
        }
    }
}
