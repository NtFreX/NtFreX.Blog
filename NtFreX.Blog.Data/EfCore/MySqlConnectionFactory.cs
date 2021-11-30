using MySql.Data.MySqlClient;
using NtFreX.Blog.Configuration;
using System;
using System.Threading.Tasks;

namespace NtFreX.Blog.Data.EfCore
{
    public class MySqlConnectionFactory : IConnectionFactory
    {
        public readonly MySqlConnection Connection;

        public MySqlTransaction Transaction { get; private set; }

        public MySqlConnectionFactory(ConfigPreloader configLoader)
        {
            Connection = new MySqlConnection(configLoader.Get(ConfigNames.MySqlDbConnectionString));
            Connection.Open();
        }
        public async Task BeginTransactionAsync()
        {
            if (Transaction != null)
                throw new Exception("A transaction is allready open");

            Transaction = await Connection.BeginTransactionAsync();
        }

        public async Task CommitTansactionAsync()
        {
            if (Transaction == null)
                throw new Exception("No transaction is open");

            await Transaction.CommitAsync();
            Transaction = null;
        }

        public async Task RollbackTansactionAsync()
        {
            if (Transaction == null)
                throw new Exception("No transaction is open");

            await Transaction.RollbackAsync();
            Transaction = null;
        }

        public void EnsureTablesExists()
        {
            RelationalDbArticleRepository.EnsureTableExists(Connection);
            RelationalDbCommentRepository.EnsureTableExists(Connection);
            RelationalDbImageRepository.EnsureTableExists(Connection);
            RelationalDbTagRepository.EnsureTableExists(Connection);
            RelationalDbVisitorRepository.EnsureTableExists(Connection);
        }
    }
}
