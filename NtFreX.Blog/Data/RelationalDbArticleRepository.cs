using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using NtFreX.Blog.Models;
using System;
using Dapper.Contrib.Extensions;

namespace NtFreX.Blog.Data
{
    /*
    
    create table if not exists `article` (
        `Id` varchar(255) not null unique, 
        `Date` DATE not null, 
        `Title` text,
        `Subtitle` text, 
        `Content` text, 
        `Published` boolean not null default 0, 
        primary key ( `Id` ) 
    );
    
    */
    public class RelationalDbArticleRepository : IArticleRepository
    {
        private readonly MySqlDatabaseConnectionFactory connectionFactory;

        public RelationalDbArticleRepository(MySqlDatabaseConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        public async Task<IReadOnlyList<ArticleModel>> FindAsync(bool includeUnpublished)
        {
            var articles = await connectionFactory.Connection.GetAllAsync<ArticleModel>();
            return articles.Where(x => includeUnpublished || x.ToDto().IsPublished()).OrderByDescending(x => x.Date).ToList();
        }

        public async Task<ArticleModel> FindByIdAsync(string id)
            => await connectionFactory.Connection.GetAsync<ArticleModel>(id);

        public async Task<string> InsertArticleAsync()
        {
            var article = new ArticleModel { Id = Guid.NewGuid().ToString() };
            await connectionFactory.Connection.InsertAsync(article);
            return article.Id.ToString();
        }

        public async Task UpdateAsync(ArticleModel model)
            => await connectionFactory.Connection.UpdateAsync(model);
    }
}
