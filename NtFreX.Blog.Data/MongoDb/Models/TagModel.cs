using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NtFreX.Blog.Data.MongoDb.Models
{
    public class TagModel : IMongoDbModel
    {
        [BsonElement("id", Order = 0)]
        public ObjectId Id { get; set; }

        [BsonElement("article_id", Order = 1)]
        public string ArticleId { get; set; }

        [BsonElement("name", Order = 2)]
        public string Name { get; set; }
    }
}
