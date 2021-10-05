using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;
using MongoDB.Driver;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Data
{
    /*

    create table if not exists `visitor` (
            `Id` varchar(255) not null unique, 
            `Date` DATE not null, 
            `UserAgent` text,
            `Article` varchar(255), 
            `RemoteIp` varchar(255), 
            primary key ( `Id` ) 
    );

    */
    public class RelationalDbVisitorRepository : VisitorRepository
    {
        private readonly MySqlDatabaseConnectionFactory connectionFactory;

        public RelationalDbVisitorRepository(MySqlDatabaseConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        public override async Task<long> CountAsync(string id)
            => (await connectionFactory.Connection.GetAllAsync<VisitorModel>()).ToList().Where(x => x.Article == id).Count(DoCountVisitor);

        public override async Task InsertAsync(VisitorModel model)
        {
            model.Id = Guid.NewGuid().ToString();
            await connectionFactory.Connection.InsertAsync(model);
        }
    }
}
