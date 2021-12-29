using System.Collections.Generic;
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
    public class RelationalDbVisitorRepository : RelationalDbRepository<Models.VisitorModel, VisitorModel>, IVisitorRepository
    {
        private readonly MySqlConnectionFactory connectionFactory;
        private readonly IMapper mapper;
        private readonly ApplicationContextActivityDecorator applicationContextActivityDecorator;

        public RelationalDbVisitorRepository(MySqlConnectionFactory connectionFactory, IMapper mapper, ApplicationContextActivityDecorator applicationContextActivityDecorator)
            : base(connectionFactory, mapper, applicationContextActivityDecorator)
        {
            this.connectionFactory = connectionFactory;
            this.mapper = mapper;
            this.applicationContextActivityDecorator = applicationContextActivityDecorator;
        }

        public async Task<long> CountByArticleIdAsync(string id)
        {
            var activity = applicationContextActivityDecorator.StartActivity($"{nameof(RelationalDbVisitorRepository)}.{nameof(CountByArticleIdAsync)}");
            var visitors = await connectionFactory.Connection.GetAllAsync<Models.VisitorModel>();
            var models = mapper.Map<List<VisitorModel>>(visitors);
            return models.Where(x => x.Article == id).Count(IVisitorRepository.ShouldCountVisitor);
        }

        public static void EnsureTableExists(MySqlConnection connection)
        {
            connection.Execute(@"
create table if not exists `visitor` (
        `Id` varchar(255) not null unique, 
        `Date` DATETIME not null, 
        `UserAgent` text,
        `Article` varchar(255), 
        `RemoteIp` varchar(255), 
        primary key ( `Id` ) 
);");
        }
    }
}
