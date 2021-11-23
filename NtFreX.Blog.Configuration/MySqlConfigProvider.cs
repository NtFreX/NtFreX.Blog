using Dapper;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace NtFreX.Blog.Configuration
{
    public class MySqlConfigProvider : IConfigProvider
    {
        // create table if not exists `configuration` ( `key` varchar(255) not null unique, `value` longtext, primary key ( `key` ) );

        private readonly MySqlConnection connection;

        public MySqlConfigProvider(string user, string pw, string server)
        {
            connection = new MySqlConnection($"Server={server};Database=configuration;Uid={user};Pwd={pw};");
        }

        public async Task<string> GetAsync(string key)
            => (await connection.QueryAsync<string>("SELECT `value` FROM `configuration` WHERE `key` = @key LIMIT 1", new { key })).First();
    }
}
