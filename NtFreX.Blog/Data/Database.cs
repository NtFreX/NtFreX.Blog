using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace NtFreX.Blog.Data
{
    public class Database
    {
        public IMongoDatabase Blog { get; }
        public IMongoDatabase Monitoring { get; }

        public Database(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetConnectionString("MongoDb"));
            Blog = client.GetDatabase("blog");
            Monitoring = client.GetDatabase("monitoring");
        }
    }
}
