using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using NtFreX.Blog.Models;
using System;
using Dapper.Contrib.Extensions;
using MySql.Data.MySqlClient;
using Dapper;
using AutoMapper;

namespace NtFreX.Blog.Data.EfCore
{
    public class RelationalDbArticleRepository : RelationalDbRepository<Models.ArticleModel, ArticleModel>, IArticleRepository
    {
        private readonly MySqlDatabaseConnectionFactory connectionFactory;
        private readonly IMapper mapper;

        public RelationalDbArticleRepository(MySqlDatabaseConnectionFactory connectionFactory, IMapper mapper)
            : base(connectionFactory, mapper)
        {
            this.connectionFactory = connectionFactory;
            this.mapper = mapper;
        }

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
