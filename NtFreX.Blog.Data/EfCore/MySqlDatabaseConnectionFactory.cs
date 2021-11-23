using MySql.Data.MySqlClient;
using NtFreX.Blog.Configuration;

namespace NtFreX.Blog.Data.EfCore
{
    public class MySqlDatabaseConnectionFactory
    {
        public readonly MySqlConnection Connection;

        public MySqlDatabaseConnectionFactory(ConfigPreloader configLoader)
        {
            Connection = new MySqlConnection(configLoader.Get(ConfigNames.MySqlDbConnectionString));
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
