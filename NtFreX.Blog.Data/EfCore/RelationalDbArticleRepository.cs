using NtFreX.Blog.Models;
using MySql.Data.MySqlClient;
using Dapper;
using AutoMapper;
using NtFreX.Blog.Configuration;

namespace NtFreX.Blog.Data.EfCore
{
    public class RelationalDbArticleRepository : RelationalDbRepository<Models.ArticleModel, ArticleModel>, IArticleRepository
    {
        public RelationalDbArticleRepository(MySqlConnectionFactory connectionFactory, IMapper mapper, ApplicationContextActivityDecorator applicationContextActivityDecorator)
            : base(connectionFactory, mapper, applicationContextActivityDecorator)
        { }

        public static void EnsureTableExists(MySqlConnection connection)
        {
            connection.Execute(@"
create table if not exists `article` (
    `Id` varchar(255) not null unique, 
    `Date` DATETIME not null, 
    `Title` text,
    `Subtitle` text, 
    `Content` text, 
    `Published` boolean not null default 0, 
    primary key ( `Id` ) 
);");
        }
    }
}
