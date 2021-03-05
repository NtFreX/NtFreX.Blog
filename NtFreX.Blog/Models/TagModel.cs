using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NtFreX.Blog.Models
{
    public class TagModel
    {
        [BsonElement("id", Order = 0)]
        public ObjectId Id { get; set; }

        [BsonElement("article_id", Order = 1)]
        public ObjectId ArticleId { get; set; }

        [BsonElement("name", Order = 2)]
        public string Name { get; set; }
    }
}
