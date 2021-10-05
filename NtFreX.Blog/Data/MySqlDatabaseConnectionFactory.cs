using MySql.Data.MySqlClient;
using NtFreX.Blog.Configuration;
using NtFreX.ConfigFlow.DotNet;

namespace NtFreX.Blog.Data
{
    public class MySqlDatabaseConnectionFactory
    {
        public readonly MySqlConnection Connection;

        public MySqlDatabaseConnectionFactory(ConfigPreloader configLoader)
        {
            Connection = new MySqlConnection(configLoader.Get(ConfigNames.MySqlDbConnectionString));
        }
    }
}
