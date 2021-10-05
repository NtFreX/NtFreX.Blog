using System.Threading.Tasks;
using MongoDB.Driver;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Data
{
    public class MongoDbImageRepository : IImageRepository
    {

        private IMongoCollection<ImageModel> collection;

        public MongoDbImageRepository(MongoDatabase database)
        {
            collection = database.Blog.GetCollection<ImageModel>("image");
        }

        public async Task<ImageModel> FindByName(string name)
            => await collection.Find(d => d.Name == name).FirstAsync();

        public async Task InsertOrUpdate(ImageModel image)
        {
            await collection.FindOneAndDeleteAsync(Builders<ImageModel>.Filter.Eq(x => x.Name, image.Name));
            await collection.InsertOneAsync(image);
        }
    }
}
