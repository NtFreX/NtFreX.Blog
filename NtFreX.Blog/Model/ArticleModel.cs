using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace NtFreX.Blog.Model
{
    public class ArticleModel
    {
        [BsonElement("id", Order = 0)]
        public ObjectId Id { get; set; }

        [BsonElement("date", Order = 1)]
        public DateTime Date { get; set; }

        [BsonElement("title", Order = 2)]
        public string Title { get; set; }

        [BsonElement("subtitle", Order = 3)]
        public string Subtitle { get; set; }

        [BsonElement("content", Order = 4)]
        public string Content { get; set; }

        [BsonElement("published", Order = 5)]
        public bool Published { get; set; }
    }
}
