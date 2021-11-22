using MongoDB.Driver;
using NtFreX.Blog.Configuration;

namespace NtFreX.Blog.Data.MongoDb
{
    public class MongoDatabase
    {
        public IMongoDatabase Blog { get; }

        public MongoDatabase(ConfigPreloader config)
        {
            var client = new MongoClient(config.Get(ConfigNames.MongoDbConnectionString));
            Blog = client.GetDatabase(config.Get(ConfigNames.BlogDatabaseName));
        }
    }
}
