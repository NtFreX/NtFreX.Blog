using MongoDB.Driver;
using NtFreX.ConfigFlow.DotNet;

namespace NtFreX.Blog.Data
{
    public class Database
    {
        public IMongoDatabase Blog { get; }
        public IMongoDatabase Monitoring { get; }

        public Database(ConfigLoader config)
        {
            var client = new MongoClient(config.Get(ConfigNames.MongoDbConnectionString));
            Blog = client.GetDatabase(config.Get(ConfigNames.BlogDatabase));
            Monitoring = client.GetDatabase(config.Get(ConfigNames.MonitoringDatabase));
        }
    }
}
