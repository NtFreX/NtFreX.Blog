using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace NtFreX.Blog.Data.MongoDb.Models
{
    public class VisitorModel : IMongoDbModel
    {
        [BsonElement("id", Order = 0)]
        public ObjectId Id { get; set; }

        [BsonElement("date", Order = 1)]
        public DateTime Date { get; set; }

        [BsonElement("user_agent", Order = 2)]
        public string UserAgent { get; set; }

        [BsonElement("article", Order = 3)]
        public string Article { get; set; }

        [BsonElement("remote_ip", Order = 5)]
        public string RemoteIp { get; set; }
    }
}