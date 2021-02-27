using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NtFreX.Blog.Model
{
    public class ImageModel
    {
        [BsonElement("id", Order = 0)]
        public ObjectId Id { get; set; }

        [BsonElement("name", Order = 1)]
        public string Name { get; set; }

        [BsonElement("type", Order = 2)]
        public string Type { get; set; }

        [BsonElement("data", Order = 3)]
        public string Data { get; set; }
    }
}
