using MongoDB.Bson;

namespace NtFreX.Blog.Data
{
    public interface IMongoDbModel
    {
        public ObjectId Id { get; set; }
    }
}
