using MongoDB.Driver;
using NtFreX.Blog.Configuration;
using System;
using System.Threading.Tasks;

namespace NtFreX.Blog.Data.MongoDb
{
    public class MongoConnectionFactory : IConnectionFactory
    {
        public MongoClient Client { get; }
        public IMongoDatabase Blog { get; }

        public IClientSessionHandle Session { get; private set; }

        public MongoConnectionFactory(ConfigPreloader config)
        {
            Client = new MongoClient(config.Get(ConfigNames.MongoDbConnectionString));
            Blog = Client.GetDatabase(config.Get(ConfigNames.BlogDatabaseName));
        }

        public async Task BeginTransactionAsync()
        {
            if (Session != null)
                throw new Exception("A transaction is allready open");

            Session = await Client.StartSessionAsync();
        }

        public async Task CommitTansactionAsync()
        {
            if (Session == null)
                throw new Exception("No transaction is open");

            await Session.CommitTransactionAsync();
            Session = null;
        }

        public async Task RollbackTansactionAsync()
        {
            if (Session == null)
                throw new Exception("No transaction is open");

            await Session.AbortTransactionAsync();
            Session = null;
        }
    }
}
